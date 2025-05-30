CREATE TRIGGER trg_AfterDelete_Transaksi
ON Transaksi
AFTER DELETE
AS
BEGIN
    INSERT INTO table_log (aksi, tabel, id_tabel, tanggal)
    SELECT 
        'DELETE' AS aksi,
        'Transaksi' AS tabel,
        inserted.id_transaksi AS id_tabel,
        GETDATE() AS tanggal
    FROM inserted;
END