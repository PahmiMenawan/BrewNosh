CREATE TRIGGER trg_AfterUpdate_Transaksi
ON Transaksi
AFTER UPDATE
AS
BEGIN
    INSERT INTO table_log (aksi, tabel, id_tabel, tanggal)
    SELECT 
        'UPDATE' AS aksi,
        'Transaksi' AS tabel,
        inserted.id_transaksi AS id_tabel,
        GETDATE() AS tanggal
    FROM inserted;
END
