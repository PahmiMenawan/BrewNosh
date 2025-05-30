CREATE TRIGGER trg_AfterInsert_Produk
ON Produk
AFTER INSERT
AS
BEGIN
    INSERT INTO table_log (aksi, tabel, id_tabel, tanggal)
    SELECT 
        'INSERT' AS aksi,
        'Produk' AS tabel,
        inserted.id_transaksi AS id_tabel,
        GETDATE() AS tanggal
    FROM inserted;
END