Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Text.Json
Imports System.Net.Http
Imports System.Net
Imports System.Net.Mail

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim ime, prezime, email As String


        ime = TextBox1.Text
        prezime = TextBox2.Text
        email = TextBox3.Text

        If ime = "" Then
            MessageBox.Show("Ime nije uneseno!")
            TextBox1.Focus()
            Exit Sub
        ElseIf prezime = "" Then
            MessageBox.Show("Prezime nije uneseno!")
            TextBox2.Focus()
            Exit Sub
        ElseIf Not IsValidEmail(email) Then
            MessageBox.Show("Nije unesen ispravan e-mail!")
            TextBox3.Focus()
            Exit Sub
        End If


        If isTimerRunning Then
            MessageBox.Show("Dozvoljen samo jedan upis u minuti! Molim pričekajte " +
                            haltTime.ToString +
                            " sekundi za novi upis!", "Upozorenje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Else
            'zapis u bazu SQL servera
            InsertToDatabase(ime, prezime, email)

            StartTimer()
        End If
    End Sub

    Private Function IsValidEmail(email As String) As Boolean
        Dim pattern As String = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
        Dim regex As New Regex(pattern)

        Return regex.IsMatch(email)
    End Function

    Private Shared Async Sub InsertToDatabase(ime As String, prezime As String, email As String)
        Dim connectionString As String = "Data Source=DESKTOP-C079G1P\MSSQLSERVER2;Initial Catalog=VisualBasic;Integrated Security=True"

        Dim url As String = "https://jsonplaceholder.typicode.com/users?email=" + email
        Dim jsonString As String = Await FetchJsonAsync(url)

        Dim user As List(Of User) = JsonSerializer.Deserialize(Of List(Of User))(jsonString)

        If user.Any Then
            For Each item As User In user
                'Dim query As String = "INSERT INTO Klijenti (ime, prezime, email) VALUES (@Value1, @Value2, @Value3)"
                Dim query As String
                Dim addressID, companyID As Integer
                Using connection As New SqlConnection(connectionString)
                    query = "INSERT INTO Address (street, suite, city, zipcode, lat, lng) VALUES (@Street, @Suite, @City, @ZipCode, @Lat, @Lng)"
                    Using command As New SqlCommand(query, connection)
                        command.Parameters.AddWithValue("@Street", item.address.street)
                        command.Parameters.AddWithValue("@Suite", item.address.suite)
                        command.Parameters.AddWithValue("@City", item.address.city)
                        command.Parameters.AddWithValue("@ZipCode", item.address.zipcode)
                        command.Parameters.AddWithValue("@Lat", item.address.geo.lat)
                        command.Parameters.AddWithValue("@Lng", item.address.geo.lng)

                        connection.Open()
                        command.ExecuteNonQuery()
                    End Using

                    query = "INSERT INTO Company (name, catchPhrase, bs) VALUES (@Name, @CatchPhrase, @Bs)"
                    Using command As New SqlCommand(query, connection)
                        command.Parameters.AddWithValue("@Name", item.company.name)
                        command.Parameters.AddWithValue("@CatchPhrase", item.company.catchPhrase)
                        command.Parameters.AddWithValue("@Bs", item.company.bs)

                        command.ExecuteNonQuery()
                    End Using

                    query = "SELECT addressID FROM Address WHERE street='" + item.address.street + "' AND suite='" + item.address.suite + "' AND city='" + item.address.city + "' AND zipcode='" + item.address.zipcode + "'"
                    Using command As New SqlCommand(query, connection)
                        Using reader As SqlDataReader = command.ExecuteReader()
                            reader.Read()
                            addressID = reader("addressID")
                        End Using
                    End Using

                    query = "SELECT companyID FROM Company WHERE name='" + item.company.name + "'"
                    Using command As New SqlCommand(query, connection)
                        Using reader As SqlDataReader = command.ExecuteReader()
                            reader.Read()
                            companyID = reader("companyID")
                        End Using
                    End Using

                    query = "INSERT INTO Klijenti (ime, prezime, email, username, phone, website, addressID, companyID) Values (@Ime, @Prezime, @Email, @Username, @Phone, @Website, @AddressID, @CompanyID)"
                    Using command As New SqlCommand(query, connection)
                        command.Parameters.AddWithValue("@Ime", ime)
                        command.Parameters.AddWithValue("@Prezime", prezime)
                        command.Parameters.AddWithValue("@Email", email)
                        command.Parameters.AddWithValue("@Username", item.username)
                        command.Parameters.AddWithValue("@Phone", item.phone)
                        command.Parameters.AddWithValue("@Website", item.website)
                        command.Parameters.AddWithValue("@AddressID", addressID)
                        command.Parameters.AddWithValue("@CompanyID", companyID)

                        command.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Dodani podaci: " + Environment.NewLine +
                                    "ime: " + ime + Environment.NewLine +
                                    "prezime: " + prezime + Environment.NewLine +
                                    "E-mail: " + email + Environment.NewLine +
                                    "Adresa: " + item.address.street + ", " + item.address.suite + ", " + item.address.city + ", " + item.address.zipcode + Environment.NewLine +
                                    "Web stranica: " + item.website + Environment.NewLine +
                                    "Telefon: " + item.phone)

                    SendEmail(ime, prezime, email, item)
                End Using
            Next
        Else
            Dim query As String = "INSERT INTO Klijenti (ime, prezime, email) VALUES (@Ime, @Prezime, @Email)"

            Using connection As New SqlConnection(connectionString)
                Using command As New SqlCommand(query, connection)
                    command.Parameters.AddWithValue("@Ime", ime)
                    command.Parameters.AddWithValue("@Prezime", prezime)
                    command.Parameters.AddWithValue("@Email", email)

                    connection.Open()
                    command.ExecuteNonQuery()
                End Using

                MessageBox.Show("Dodani podaci: " + Environment.NewLine +
                                "ime: " + ime + Environment.NewLine +
                                "prezime: " + prezime + Environment.NewLine +
                                "E-mail: " + email)

                SendEmail(ime, prezime, email, Nothing)
            End Using
        End If

    End Sub

    Private Shared Sub SendEmail(ime As String, prezime As String, email As String, item As User)
        Dim smtpServer As String = "smtp.example.com" 'Unijeti adresu SMTP servera
        Dim smtpPort As Integer = 587 'Unijeti SMTP port
        Dim smtpUsername As String = "vas@email.com" 'Unijeti SMTP username
        Dim smtpPassword As String = "vaspassword" 'Unijeti SMTP password

        Dim fromAddress As New MailAddress("posiljatelj@example.com", "Ime Pošiljatelja") 'Unijeti podatke pošaljitelja
        Dim toAddress As New MailAddress("primatelj@example.com", "Ime Primatelja") 'Unijeti podatke primaoca

        Dim subject As String = "Dodani podaci za korisnika " + ime + " " + prezime
        Dim body As String

        If item Is Nothing Then
            body = "Dodani podaci: " + Environment.NewLine +
                   "ime: " + ime + Environment.NewLine +
                   "prezime: " + prezime + Environment.NewLine +
                   "E-mail: " + email
        Else
            body = "Dodani podaci: " + Environment.NewLine +
                   "ime: " + ime + Environment.NewLine +
                   "prezime: " + prezime + Environment.NewLine +
                   "E-mail: " + email + Environment.NewLine +
                   "Adresa: " + item.address.street + ", " + item.address.suite + ", " + item.address.city + ", " + item.address.zipcode + Environment.NewLine +
                   "Web stranica: " + item.website + Environment.NewLine +
                   "Telefon: " + item.phone
        End If

        Dim smtpClient As New SmtpClient(smtpServer, smtpPort)
        smtpClient.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
        smtpClient.EnableSsl = True

        Dim mail As New MailMessage(fromAddress, toAddress)
        mail.Subject = subject
        mail.Body = body

        Try
            smtpClient.Send(mail)
            MessageBox.Show("Email uspješno poslan.")
        Catch ex As Exception
            MessageBox.Show($"Greška: {ex.Message}")
        End Try
    End Sub

    Private Shared Async Function FetchJsonAsync(url As String) As Task(Of String)
        Using httpClient As New HttpClient()
            Try
                Dim response As HttpResponseMessage = Await httpClient.GetAsync(url)

                If response.IsSuccessStatusCode Then
                    Return Await response.Content.ReadAsStringAsync()
                Else
                    Throw New Exception($"Neuspješno dohvaćanje podataka. Status code: {response.StatusCode}")
                End If
            Catch ex As Exception
                Throw New Exception($"Greška: {ex.Message}")
            End Try
        End Using
    End Function

    Private Sub StartTimer()
        haltTime = 60
        isTimerRunning = True
        Timer1.Enabled = True
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        haltTime -= 1
        If haltTime <= 0 Then
            isTimerRunning = False
            Timer1.Enabled = False
        End If
    End Sub
End Class

Public Class User
    Public Property id As Long
    Public Property name As String
    Public Property username As String
    Public Property email As String
    Public Property address As AddressInfo
    Public Property phone As String
    Public Property website As String
    Public Property company As CompanyInfo

End Class

Public Class CompanyInfo
    Public Property name As String
    Public Property catchPhrase As String
    Public Property bs As String
End Class

Public Class AddressInfo
    Public Property street As String
    Public Property suite As String
    Public Property city As String
    Public Property zipcode As String
    Public Property geo As Geoinfo
End Class

Public Class Geoinfo
    Public Property lat As String
    Public Property lng As String
End Class
