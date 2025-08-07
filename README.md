SNMP Trap Receiver - Redis Entegrasyonu (Ubuntu .NET Worker Service)

Bu proje, Ubuntu üzerinde çalışan bir .NET Worker Service uygulamasıdır. Amaç, SNMP protokolü üzerinden gelen trap mesajlarını dinlemek ve bu verileri Redis veritabanına kaydetmektir.

Kullanılan Teknolojiler:
-.NET 7.0 (Worker Service)
-Redis (Veritabanı)
-SnmpSharpNet (Trap dinleme)
-StackExchange.Redis (Redis bağlantısı)

Sistem, RedisInsight veya redis-cli üzerinden gelen trap verilerini izlemenizi sağlar.
Bu uygulama, ağ yönetim sistemleri veya SNMP tabanlı izleme sistemleri için temel oluşturabilir.



Project Description (English)
SNMP Trap Receiver - Redis Integration (Ubuntu .NET Worker Service)

This is a .NET Worker Service application running on Ubuntu. The purpose of the project is to listen to incoming SNMP trap messages and store them into a Redis database.

Technologies Used:
-.NET 7.0 (Worker Service)
-Redis (Database)
-SnmpSharpNet (Trap Listener)
-StackExchange.Redis (Redis Connector)

 You can monitor trap data via RedisInsight or redis-cli.
This application can serve as a basic component for network management or SNMP-based monitoring systems.
