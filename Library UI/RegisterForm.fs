namespace LibraryUI

open System
open System.Windows.Forms
open System.Drawing
open LibraryUI.Database

type RegisterForm() as this =
    inherit Form(Text="Register", Width=450, Height=300)

    do
        this.BackColor <- Color.LightPink
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false

        let usernameLabel = new Label(Text="Username:", Location=Point(30,30), Size=Size(80,25))
        let usernameTextBox = new TextBox(Location=Point(120,30), Size=Size(250,25))

        let emailLabel = new Label(Text="Email:", Location=Point(30,70), Size=Size(80,25))
        let emailTextBox = new TextBox(Location=Point(120,70), Size=Size(250,25))

        let passwordLabel = new Label(Text="Password:", Location=Point(30,110), Size=Size(80,25))
        let passwordTextBox = new TextBox(Location=Point(120,110), Size=Size(250,25))
        passwordTextBox.PasswordChar <- '*'

        let registerButton = new Button(Text="Register", Location=Point(120,160), Size=Size(120,35), BackColor=Color.LightGreen)

        registerButton.Click.Add(fun _ ->
            if String.IsNullOrWhiteSpace(usernameTextBox.Text) || String.IsNullOrWhiteSpace(emailTextBox.Text) || String.IsNullOrWhiteSpace(passwordTextBox.Text) then
                MessageBox.Show("Username, Email and Password are required!") |> ignore
            else
                Database.addUser usernameTextBox.Text emailTextBox.Text passwordTextBox.Text "User"
                MessageBox.Show("User registered successfully!") |> ignore
                this.Close()
        )

        this.Controls.AddRange [| usernameLabel; usernameTextBox; emailLabel; emailTextBox;
                                  passwordLabel; passwordTextBox; registerButton |]
