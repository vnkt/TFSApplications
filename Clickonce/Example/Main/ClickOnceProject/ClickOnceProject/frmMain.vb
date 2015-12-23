Public Class frmMain

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) Then
            txtVersion.Text = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        Else
            txtVersion.Text = "Not ClickOnce Deployed"
        End If
    End Sub
End Class
