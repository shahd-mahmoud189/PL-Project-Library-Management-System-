namespace LibraryUI

open System
open System.Windows.Forms
open System.Drawing
open LibraryUI.Database
open LibraryUI.UIHelpers

type UserForm(loginForm: Form, currentUser: User) as this =
    inherit Form(Text="User Panel", Width=1000, Height=700)

    do
        this.BackColor <- Color.LightYellow
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.StartPosition <- FormStartPosition.CenterScreen

        let topPanel = new Panel()
        topPanel.Dock <- DockStyle.Top
        topPanel.Height <- 50
        topPanel.BackColor <- Color.LightBlue
        topPanel.Padding <- Padding(10)

        let searchTextBox = new TextBox()
        searchTextBox.Location <- Point(10,15)
        searchTextBox.Width <- 250

        let searchButton = new Button(Text="Search", Location=Point(270,12), Width=100)
        let borrowButton = new Button(Text="Borrow", Location=Point(380,12), Width=100)
        let returnButton = new Button(Text="Return", Location=Point(490,12), Width=100)
        let showAllButton = new Button(Text="Show All", Location=Point(600,12), Width=100)
        let logoutButton = new Button(Text="Logout", Location=Point(800,12), Width=100)

        topPanel.Controls.AddRange [| searchTextBox; searchButton; borrowButton; returnButton; showAllButton; logoutButton |]

        let booksPanel = new FlowLayoutPanel()
        booksPanel.Dock <- DockStyle.Fill
        booksPanel.AutoScroll <- true
        booksPanel.Padding <- Padding(50)
        booksPanel.BackColor <- Color.MistyRose

        let mutable selectedCard : Panel = null

        let highlight card =
            selectedCard <- card
            for c in booksPanel.Controls do
                let b = (c :?> Panel).Tag :?> Book
                (c :?> Panel).BackColor <- if b.Status = Borrowed then Color.LightCoral else Color.LightGreen
                (c :?> Panel).BorderStyle <- BorderStyle.FixedSingle
            card.BorderStyle <- BorderStyle.Fixed3D
            card.BackColor <- card.BackColor

        let loadCards() =
            booksPanel.Controls.Clear()
            for b in loadBooksFromDb() |> Seq.toList do
                let card = new Panel(Width=500, Height=100, BorderStyle=BorderStyle.FixedSingle, Margin=Padding(10))
                card.BackColor <- if b.Status = Borrowed then Color.LightCoral else Color.LightGreen

                let titleLabel = new Label(Text = sprintf "Title: %s" b.Title, Location=Point(10,10), AutoSize=true)
                let authorLabel = new Label(Text = sprintf "Author: %s" b.Author, Location=Point(10,35), AutoSize=true)
                let statusLabel = new Label(Text = sprintf "Status: %A" b.Status, Location=Point(300,10), AutoSize=true)

                card.Controls.AddRange [| titleLabel; authorLabel; statusLabel |]
                card.Tag <- b
                card.Click.Add(fun _ -> highlight card)
                booksPanel.Controls.Add(card)

        loadCards()

        borrowButton.Click.Add(fun _ ->
            if selectedCard <> null then
                let b = selectedCard.Tag :?> Book
                let count = getBorrowedCountByUser currentUser.Id
                if count >= 3 then MessageBox.Show("You can borrow max 3 books.") |> ignore
                elif b.Status = Borrowed then MessageBox.Show("Book already borrowed.") |> ignore
                else
                    borrowBook currentUser.Id b.Id
                    b.Status <- Borrowed
                    updateBookInDb b
                    MessageBox.Show("Book borrowed successfully.") |> ignore
                    loadCards()
        )

        returnButton.Click.Add(fun _ ->
            if selectedCard <> null then
                let b = selectedCard.Tag :?> Book
                if not (isBookBorrowedByUser currentUser.Id b.Id) then
                    MessageBox.Show("You didn't return this book.") |> ignore
                else
                    returnBook currentUser.Id b.Id
                    b.Status <- Available
                    updateBookInDb b
                    MessageBox.Show("Book returned successfully.") |> ignore
                    loadCards()
        )

        showAllButton.Click.Add(fun _ -> loadCards())

        searchButton.Click.Add(fun _ ->
            let q = searchTextBox.Text.Trim().ToLower()
            booksPanel.Controls.Clear()
            selectedCard <- null

            for b in loadBooksFromDb()
                     |> Seq.filter (fun b ->
                         b.Title.ToLower().Contains(q) ||
                         b.Author.ToLower().Contains(q)
                     )
                     |> Seq.toList do

                let card = new Panel(Width=500, Height=100, BorderStyle=BorderStyle.FixedSingle, Margin=Padding(10))
                card.BackColor <- if b.Status = Borrowed then Color.LightCoral else Color.LightGreen

                let titleLabel = new Label(Text = sprintf "Title: %s" b.Title, Location=Point(10,10), AutoSize=true)
                let authorLabel = new Label(Text = sprintf "Author: %s" b.Author, Location=Point(10,35), AutoSize=true)
                let statusLabel = new Label(Text = sprintf "Status: %A" b.Status, Location=Point(300,10), AutoSize=true)

                card.Controls.AddRange [| titleLabel; authorLabel; statusLabel |]
                card.Tag <- b
                card.Click.Add(fun _ -> highlight card)

                booksPanel.Controls.Add(card)
        )


        logoutButton.Click.Add(fun _ ->
            this.Hide()
            loginForm.Show()
        )

        this.Controls.AddRange [| topPanel; booksPanel |]
