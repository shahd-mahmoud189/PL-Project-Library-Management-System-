namespace LibraryUI

open System
open System.Windows.Forms
open System.Drawing
open LibraryUI.Database

type LoginForm() as this =
    inherit Form(Text="Login", Width=450, Height=300)

    let loginSuccessful = new Event<string * string>()

    do
        this.BackColor <- Color.LightBlue
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.StartPosition <- FormStartPosition.CenterScreen

        let emailLabel = new Label()
        emailLabel.Text <- "Email:"
        emailLabel.Location <- Point(30,30)
        emailLabel.Size <- Size(80,25)

        let emailTextBox = new TextBox()
        emailTextBox.Location <- Point(120,30)
        emailTextBox.Size <- Size(250,25)

        let passwordLabel = new Label()
        passwordLabel.Text <- "Password:"
        passwordLabel.Location <- Point(30,70)
        passwordLabel.Size <- Size(80,25)

        let passwordTextBox = new TextBox()
        passwordTextBox.Location <- Point(120,70)
        passwordTextBox.Size <- Size(250,25)
        passwordTextBox.PasswordChar <- '*'

        let loginButton = new Button()
        loginButton.Text <- "Login"
        loginButton.Location <- Point(50,130)
        loginButton.Size <- Size(120,35)
        loginButton.BackColor <- Color.LightGreen

        let registerButton = new Button()
        registerButton.Text <- "Register"
        registerButton.Location <- Point(200,130)
        registerButton.Size <- Size(120,35)
        registerButton.BackColor <- Color.LightYellow

        this.Controls.AddRange [| emailLabel; emailTextBox; passwordLabel; passwordTextBox; loginButton; registerButton |]

        loginButton.Click.Add(fun _ ->
            match Database.verifyLoginEmail emailTextBox.Text passwordTextBox.Text with
            | Some user ->
                loginSuccessful.Trigger(user.Email, match user.Role with Role.Admin -> "Admin" | Role.User -> "User")
            | None ->
                MessageBox.Show("Invalid credentials!") |> ignore
        )

        registerButton.Click.Add(fun _ ->
            this.Hide()
            use form = new RegisterForm()
            form.ShowDialog()
            this.Show()
        )

    member this.LoginSuccessful = loginSuccessful.Publish
