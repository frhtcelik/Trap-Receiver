using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using SnmpSharpNet;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class Worker : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log("Servis baþarýyla baþladý.");
        var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = redis.GetDatabase();

        int port = 162;
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, port));

        Log("SNMP Trap dinleniyor: UDP " + port);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                byte[] buffer = new byte[16 * 1024];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                var receiveTask = socket.ReceiveMessageFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);

                var completedTask = await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, stoppingToken));

                if (completedTask == receiveTask)
                {
                    var result = receiveTask.Result;
                    int inlen = result.ReceivedBytes;
                    remoteEP = result.RemoteEndPoint;

                    if (inlen > 0)
                    {
                        int ver = SnmpPacket.GetProtocolVersion(buffer, inlen);
                        string trapInfo = "";

                        if (ver == (int)SnmpVersion.Ver1)
                        {
                            trapInfo = $"SNMPv1 Trap from {remoteEP}";
                        }
                        else if (ver == (int)SnmpVersion.Ver2)
                        {
                            SnmpV2Packet pkt = new SnmpV2Packet();
                            pkt.decode(buffer, inlen);
                            if ((PduType)pkt.Pdu.Type == PduType.V2Trap)
                            {
                                trapInfo = $"SNMPv2 Trap from {remoteEP}: ";
                                foreach (Vb v in pkt.Pdu.VbList)
                                {
                                    trapInfo += $"{v.Oid} = {v.Value}; ";
                                }
                            }
                        }
                        else if (ver == (int)SnmpVersion.Ver3)
                        {
                            trapInfo = $"SNMPv3 Trap from {remoteEP} (detaylý çözümleme eklenebilir)";
                        }
                        else
                        {
                            trapInfo = $"Bilinmeyen SNMP versiyonu: {ver}";
                        }

                        Log(trapInfo);
                        await db.ListLeftPushAsync("snmp_traps", trapInfo);
                    }
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Log("Hata: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }

    private void Log(string message)
    {
        try
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TrapReceiverLogs");
            Directory.CreateDirectory(logDir);

            string logPath = Path.Combine(logDir, "TrapReceiverService.log");
            File.AppendAllText(logPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            // Eðer dosyaya eriþilemezse, hata detayýný Event Viewer'a yazmak da bir seçenek
            Console.WriteLine($"Log yazma hatasý: {ex.Message}");
        }
    }
}

