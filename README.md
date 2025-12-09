# SqlToPdfWithAI

📄 README.md — SqlToPdfWithAI
SqlToPdfWithAI

SQL sorgularını otomatik olarak çalıştırıp sonuçlarını grafik + tablo şeklinde profesyonel bir PDF raporuna dönüştüren ASP.NET Core uygulaması.

Bu proje, kurumsal raporlama ihtiyaçlarını minimum kullanıcı etkileşimi ile çözmek amacıyla geliştirilmiştir.
Kullanıcıdan yalnızca SELECT sorgusu alınır, gerisini sistem tamamen otomatik yapar.

🚀 Özellikler
✔ SQL → PDF Rapor Dönüşümü

Kullanıcı yalnızca SQL sorgusunu yazar.

Sistem sonucu işler, analiz eder ve PDF’e dönüştürür.

✔ Otomatik Grafik Üretimi

Sistem kolon tiplerini analiz eder ve grafiği kendi seçer:

Tarih + Sayısal → Line Chart

Kategori (string) + Sayısal → Bar Chart (Top 10)

Sayısal kolon yoksa grafik üretilmez (PDF’e bilgi mesajı eklenir)

✔ PDF İçeriği

Rapor adı

Oluşturulma tarihi

Sorgu metni

Satır sayısı, süre, kolon bilgileri

Otomatik grafik

Veri tablosu (ilk 50 kayıt)

✔ Rapor Arşivleme

Her sorgu:

benzersiz bir ReportId ile kayıt altına alınır,

İstenilen isim verilir,

JSON formatında saklanır,

istenildiğinde tekrar PDF olarak indirilebilir.

✔ Basit ve Temiz UI

Kullanıcıdan sadece:

SQL sorgusu

Rapor adı

alınır. Grafik seçimi yapılmaz — sistem kendisi en doğru grafiği üretir.

🛠 Kullanılan Teknolojiler

ASP.NET Core 9 MVC

Dapper

ScottPlot (grafik)

QuestPDF (PDF)

SQL Server

Bootstrap 5

📦 Kurulum
1️⃣ Repoyu Klonlayın
git clone https://github.com/kullaniciadi/SqlToPdfWithAI.git
cd SqlToPdfWithAI

🗄 Veritabanı Bağlantısı

Projenizi kendi SQL Server veritabanınıza bağlamak için şu dosyayı düzenleyin:

📌 appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=YOUR_DB_NAME;User Id=sa;Password=yourPassword;"
}

Açıklamalar:

Server=.; → local instance

Integrated Security=True → Windows Authentication

Uzak sunucu örneği:

"Server=192.168.1.20;Database=MyDb;User Id=sa;Password=123;"


Uygulama başka bir ayara ihtiyaç duymadan bu bağlantıyla çalışır.

📝 Loglama Neden Önemli?

Uygulamada log mekanizması, özellikle:

performans ölçümü,

hata analizleri,

kullanıcı davranış takibi

için kritik rol oynar.

✔ Loglar sayesinde:

Hangi SQL sorgusunun kaç ms sürdüğünü görürsünüz

Hata oluştuğunda tam olarak nerede olduğunu anlarsınız

Grafik oluşturma hatalarının nedeni loglarda görünür

Üretim ortamında teşhis süresi saniyelere düşer


🛡 Try/Catch Kullanımı ve Sağladığı Güvenlik

Bu projede try/catch blokları stratejik olarak kullanılmıştır.

✔ Try/Catch Ne Sağlar?

Uygulamanın çökmesini engeller

Hata detaylarını loglara düşürür

Kullanıcıya temiz mesaj verir

Geliştiricinin problemi hızlı anlamasını sağlar


📊 Otomatik Grafik Mantığı

Sistem veriyi analiz eder ve grafik tipini kendi seçer:

Şart	Grafik
Tarih + Sayısal kolon	Line Chart
String + Sayısal kolon	Bar Chart (Top 10)
Hiç sayısal kolon yok	Grafik eklenmez
🎨 Geliştirme Olanakları

Bu proje grafik motoru açısından genişletilebilir yapıdadır.

Eklenebilir Grafik Türleri:

Pie Chart

Scatter Plot

Multi-Series Line Chart

Heatmap

Box Plot

Yeni Özellik Fikirleri:

Kullanıcıya grafik teması seçtirme

Birden fazla grafik üretme

ML tabanlı “en iyi grafik” öneri sistemi

PDF tasarım şablonları

Karanlık tema / açık tema PDF üretimi

📂 Proje Yapısı
SqlToPdfWithAI/
│
├── Controllers/
│   ├── QueryController.cs
│   └── ReportController.cs
│
├── Services/
│   ├── ChartHelper.cs
│   ├── PdfHelper.cs
│   └── DbQueryService.cs
│
├── Dtos/
│   ├── QueryRequestDto.cs
│   ├── QueryPersistModelDto.cs
│   └── QueryResponseDto.cs
│
├── Views/
│   └── Home/Index.cshtml
│
└── storage/
    ├── *.json
    ├── *chart1.png
    └── *.pdf


▶ Kullanım
1️⃣ SQL sorgusunu yaz
2️⃣ Rapor adını gir
3️⃣ “Rapor Al” butonuna bas
4️⃣ “PDF indir” butonuyla raporu indir

Sistem veri yapısına göre grafik türünü otomatik belirler.Lisans

MIT License
