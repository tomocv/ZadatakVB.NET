CREATE VIEW [Biz e-mails] AS
SELECT ime, prezime, email
FROM Klijenti
WHERE RIGHT(email, 4) = '.biz';