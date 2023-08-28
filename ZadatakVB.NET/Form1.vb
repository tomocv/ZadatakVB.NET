Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If isTimerRunning Then
            MessageBox.Show("Dozvoljen samo jedan upis u minuti! Molim pričekajte " + haltTime.ToString + " sekundi za novi upis!")
        Else
            ' TODO implementacija zapisa u bazu
            haltTime = 60
            isTimerRunning = True
            Timer1.Enabled = True
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        haltTime -= 1
        If haltTime <= 0 Then
            isTimerRunning = False
            Timer1.Enabled = False
        End If
    End Sub
End Class
