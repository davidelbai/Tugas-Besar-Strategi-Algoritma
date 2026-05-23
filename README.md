# Tugas Besar Strategi Algoritma — Robocode Tank Royale
**Tim 3D**  
Mata Kuliah: IF25--21013 Strategi Algoritma  
Game Engine: Tank Royale (versi modifikasi asisten)  
Bahasa: C# (.NET 6.0)  
BotApi: `Robocode.TankRoyale.BotApi` v0.30.0

---

## Daftar Bot

| No | Nama Bot | Tipe | Strategi Greedy |
|----|----------|------|-----------------|
| 1 | **AggressorBot** | **Utama** | Aggressive Pursuit & Dynamic Firepower |
| 2 | SniperBot | Alternatif 1 | Expected Gain Maximization + Linear Targeting |
| 3 | SurvivorBot | Alternatif 2 | Maximum Survival Score |
| 4 | RammerBot | Alternatif 3 | Weakest Target Ram + Bullet Combo |

---

## Cara Menjalankan Bot

### Prasyarat
- .NET 6.0 SDK sudah terinstall
- Game engine Tank Royale (versi modifikasi) sudah berjalan
- Server game aktif (default: `ws://localhost:7654`)

### Langkah-langkah

1. Ekstrak ZIP ke folder pilihan, misalnya `TubesBesar_3D_Bots/`
2. Buka terminal / PowerShell
3. Masuk ke folder bot yang ingin dijalankan:
   ```
   cd bots/AggressorBot
   ```
4. Jalankan dengan:
   ```
   dotnet run
   ```
5. Bot akan otomatis terhubung ke server game engine

> **Catatan:** Jalankan setiap bot di terminal terpisah. Pastikan game engine sudah berjalan sebelum menjalankan bot.

### Menjalankan semua bot sekaligus (Windows — 4 terminal)
```powershell
Start-Process powershell -ArgumentList "cd bots\AggressorBot; dotnet run"
Start-Process powershell -ArgumentList "cd bots\SniperBot; dotnet run"
Start-Process powershell -ArgumentList "cd bots\SurvivorBot; dotnet run"
Start-Process powershell -ArgumentList "cd bots\RammerBot; dotnet run"
```

---

## Penjelasan Strategi Greedy

Setiap bot mengimplementasikan **algoritma Greedy** dengan heuristik yang berbeda-beda. Greedy berarti: di setiap giliran (turn), bot memilih aksi yang memberikan keuntungan terbesar **secara lokal/langsung**, tanpa merencanakan ke depan.

---

### 1. AggressorBot *(Bot Utama)*

**Fungsi Objektif:** Memaksimalkan total *Bullet Damage* dan *Bullet Damage Bonus*

**Heuristik Greedy:**
> Pada setiap turn saat musuh terdeteksi, pilih firepower tertinggi yang masih menguntungkan secara energi, lalu kejar musuh secara langsung.

**Keputusan Greedy per Turn:**

| Kondisi Energi Kita | Firepower Dipilih | Alasan |
|---|---|---|
| Energi > 50 | 3.0 (maksimum) | Cadangan cukup, damage maksimal |
| Energi 20–50 | 1.5 (menengah) | Jaga keseimbangan damage vs energi |
| Energi ≤ 20 | 0.5 (minimal) | Mode hemat, hindari kehabisan energi |

**Keputusan Greedy Pergerakan:**
- Jika jarak ke musuh > 200 px → maju mendekati musuh
- Jika jarak ≤ 200 px → lingkari musuh (sulit ditembak)

**Skor yang Dioptimalkan:**
- `Bullet Damage`: firepower besar = damage besar
- `Bullet Damage Bonus`: +20% dari damage jika berhasil membunuh

---

### 2. SniperBot *(Alternatif 1)*

**Fungsi Objektif:** Memaksimalkan *Expected Gain* per tembakan

**Heuristik Greedy:**
> Evaluasi setiap pilihan firepower (0.5, 1.0, 1.5, 2.0, 2.5, 3.0) dengan rumus ExpectedGain, dan pilih yang tertinggi. Jaga jarak optimal 250–500 px.

**Rumus Expected Gain:**
```
bulletSpeed  = 20 - (3 × firepower)
timeToHit    = jarak / bulletSpeed
hitChance    = max(0.1, 1.0 - (timeToHit × 0.05))
ExpectedGain = (3 × firepower × hitChance) - firepower
```
Peluru ringan = lebih cepat = `hitChance` lebih tinggi = bisa lebih menguntungkan dari peluru berat di jarak jauh.

**Linear Targeting (Prediksi Posisi Musuh):**
```
predictedX = musuhX + sin(arahMusuh) × kecepatanMusuh × timeToHit
predictedY = musuhY + cos(arahMusuh) × kecepatanMusuh × timeToHit
```
Meriam diarahkan ke posisi *prediksi*, bukan posisi saat ini.

**Keputusan Greedy Pergerakan:**
- Jarak < 250 px → mundur (terlalu dekat)
- Jarak > 500 px → maju sedikit (terlalu jauh)
- Jarak 250–500 px → gerak lateral (dalam zona optimal)

**Skor yang Dioptimalkan:**
- `Bullet Damage` + `Bullet Damage Bonus` dengan tembakan presisi
- `Survival Score` dengan menjaga jarak aman

---

### 3. SurvivorBot *(Alternatif 2)*

**Fungsi Objektif:** Memaksimalkan *Survival Score* dan *Last Survival Bonus*

**Heuristik Greedy:**
> Pada setiap turn, pilih gerakan yang menjauhi ancaman terdekat. Tembak hanya jika aman dan menguntungkan.

**Deteksi Ancaman — Enemy Fire Detection:**
Bot mendeteksi apakah musuh baru saja menembak dengan memantau **penurunan energi musuh**:
```
energyDrop = energiMusuhSebelumnya - energiMusuhSekarang
jika (energyDrop > 0 dan energyDrop <= 3.0) → musuh baru tembak!
```
Jika terdeteksi, bot langsung melakukan evade tegak lurus.

**Keputusan Greedy Pergerakan:**
- Jarak musuh < 200 px (BAHAYA) → flee (lari menjauhi)
- Jarak musuh 200–400 px → lateral movement
- Tidak ada musuh terdeteksi → circling arena

**Keputusan Greedy Menembak:**
- Hanya tembak jika: jarak < 300 px DAN energi kita > 30
- Firepower rendah (0.5–1.0) untuk hemat energi

**Skor yang Dioptimalkan:**
- `Survival Score`: +50 poin setiap ada bot lain yang mati
- `Last Survival Bonus`: +10 × jumlah musuh jika jadi yang terakhir hidup

---

### 4. RammerBot *(Alternatif 3)*

**Fungsi Objektif:** Memaksimalkan *Ram Damage* dan *Ram Damage Bonus*

**Heuristik Greedy:**
> Dari semua musuh yang terdeteksi, selalu kejar musuh dengan **energi paling rendah** karena paling mudah dibunuh untuk mendapat Ram Damage Bonus 30%.

**Greedy Target Selection:**
```
for setiap musuh yang diketahui:
    if musuh.energi < energiTerendah:
        pilih musuh ini sebagai target
```
Target diupdate setiap kali radar mendeteksi musuh baru.

**Tracking Musuh:**
Bot menyimpan data semua musuh yang pernah terdeteksi dalam `Dictionary<int, (x, y, energy)>`, sehingga bisa memilih target terbaik meski tidak semua musuh terlihat di waktu bersamaan.

**Combo Strategi:**
Sambil mengejar target, bot juga menembak dengan firepower rendah (0.5) — peluru ringan bergerak cepat sehingga lebih banyak mengenai target yang sedang dikejar.

**Skor yang Dioptimalkan:**
- `Ram Damage`: 2× dari damage yang dibuat saat menabrak
- `Ram Damage Bonus`: +30% dari damage jika musuh mati karena ditabrak
- `Bullet Damage` + `Bullet Damage Bonus`: dari combo tembakan saat mengejar

---

## Komponen Skor (Referensi)

| Komponen | Nilai |
|---|---|
| Bullet Damage | = damage yang dibuat ke musuh |
| Bullet Damage Bonus | +20% dari damage jika kill dengan peluru |
| Survival Score | +50 per bot lain yang mati |
| Last Survival Bonus | +10 × jumlah musuh (jika jadi yang terakhir) |
| Ram Damage | 2× damage dari menabrak |
| Ram Damage Bonus | +30% dari damage jika kill dengan tabrak |

---

## Struktur File

```
bots/
├── README.md                      <- file ini
├── AggressorBot/
│   ├── AggressorBot.cs            <- kode utama bot
│   ├── AggressorBot.csproj        <- konfigurasi project .NET
│   ├── AggressorBot.json          <- metadata bot (nama, versi, author)
│   ├── AggressorBot.cmd           <- script jalankan (Windows)
│   └── AggressorBot.sh            <- script jalankan (Linux/Mac)
├── SniperBot/
│   ├── SniperBot.cs
│   ├── SniperBot.csproj
│   ├── SniperBot.json
│   ├── SniperBot.cmd
│   └── SniperBot.sh
├── SurvivorBot/
│   ├── SurvivorBot.cs
│   ├── SurvivorBot.csproj
│   ├── SurvivorBot.json
│   ├── SurvivorBot.cmd
│   └── SurvivorBot.sh
└── RammerBot/
    ├── RammerBot.cs
    ├── RammerBot.csproj
    ├── RammerBot.json
    ├── RammerBot.cmd
    └── RammerBot.sh
```

---

## Dependencies

```xml
<PackageReference Include="Robocode.TankRoyale.BotApi" Version="0.30.0"/>
<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.2"/>
```

---

*IF25--21013 Strategi Algoritma — Tugas Besar | Tim 3D*
