Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

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

    Private Shared Sub InsertToDatabase(ime As String, prezime As String, email As String)
        Dim connectionString As String = "Data Source=DESKTOP-C079G1P\MSSQLSERVER2;Initial Catalog=VisualBasic;Integrated Security=True"
        Dim query As String = "INSERT INTO Klijenti (ime, prezime, email) VALUES (@Value1, @Value2, @Value3)"

        Using connection As New SqlConnection(connectionString)
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@Value1", ime)
                command.Parameters.AddWithValue("@Value2", prezime)
                command.Parameters.AddWithValue("@Value3", email)

                connection.Open()
                command.ExecuteNonQuery()
                MessageBox.Show($"Dodani podaci:{Environment.NewLine}ime: {ime}{Environment.NewLine}prezime: {prezime}{Environment.NewLine}E-mail: {email}")
            End Using
        End Using
    End Sub

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
