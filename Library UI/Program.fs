namespace LibraryUI

open System
open System.Windows.Forms
open LibraryUI.Database
open LibraryUI.UIHelpers

module Program =

    [<STAThread>]
    do
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(false)

        use loginForm = new LoginForm()

        loginForm.LoginSuccessful.Add(fun (email, role) ->
            match role with
            | "Admin" ->
                let adminForm = new AdminForm(loginForm)
                adminForm.Show()
            | "User" ->
                match Database.getUserByEmail email with
                | Some user ->
                    let userForm = new UserForm(loginForm, user)
                    userForm.Show()
                | None -> ()
            | _ -> ()

            loginForm.Hide()
        )

        Application.Run(loginForm)
