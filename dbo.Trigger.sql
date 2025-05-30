CREATE TRIGGER trg_AfterDelete_Transaksi
ON Transaksi
AFTER INSERT
AS
BEGIN
    INSERT INTO table_log (aksi, tabel, tanggal)
    SELECT 
        'INSERT' AS aksi,
        'Transaksi' AS tabel,
        GETDATE() AS tanggal
    FROM inserted;
END