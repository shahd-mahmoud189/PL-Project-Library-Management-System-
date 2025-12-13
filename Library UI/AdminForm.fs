namespace LibraryUI

open System
open System.Windows.Forms
open System.Drawing
open LibraryUI.Database
open LibraryUI.UIHelpers

type AdminForm(loginForm: Form) as this =
    inherit Form(Text="Admin Panel", Width=1000, Height=800)

    do
        this.BackColor <- Color.LightPink
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.StartPosition <- FormStartPosition.CenterScreen

        let topPanel = new Panel(Dock=DockStyle.Top, Height=60)
        topPanel.BackColor <- Color.LightCoral
        topPanel.Padding <- Padding(10)

        let searchTextBox = new TextBox(Location=Point(10, 15), Width=300)
        let searchButton = new Button(Text="Search", Location=Point(320, 12), Width=80, BackColor=Color.PeachPuff)
        let showAllButton = new Button(Text="Show All", Location=Point(410, 12), Width=80, BackColor=Color.Linen)

        let logoutButton = new Button(Text="Logout", Width=80, BackColor=Color.LightGray)
        logoutButton.Dock <- DockStyle.Right

        topPanel.Controls.AddRange [| searchTextBox; searchButton; showAllButton |]

        let addPanel = new Panel(Dock=DockStyle.Top, Height=80)
        addPanel.Padding <- Padding(10)
        addPanel.BackColor <- Color.LightYellow

        let titleLabel = new Label(Text="Book Title:", Location=Point(10,0), AutoSize=true)
        let titleTextBox = new TextBox(Location=Point(10,20), Width=250)

        let authorLabel = new Label(Text="Author:", Location=Point(270,0), AutoSize=true)
        let authorTextBox = new TextBox(Location=Point(270,20), Width=250)

        let addButton = new Button(Text="Add Book", Location=Point(530,18), Size=Size(100,25), BackColor=Color.LightGreen)

        addPanel.Controls.AddRange [| titleLabel; titleTextBox; authorLabel; authorTextBox; addButton; logoutButton |]

        let cardsPanel = new FlowLayoutPanel(Dock=DockStyle.Fill, AutoScroll=true)
        cardsPanel.Padding <- Padding(20, 150, 20, 20)  
        cardsPanel.WrapContents <- true
        cardsPanel.FlowDirection <- FlowDirection.LeftToRight
        cardsPanel.BackColor <- Color.MistyRose
        cardsPanel.AutoScroll <- true

        let rec loadCards() =
            cardsPanel.Controls.Clear()
            let allBooks = Database.loadBooksFromDb() |> Seq.toList
            for b in allBooks do
                let card = UIHelpers.createCardWithSelection b
                                (fun book ->
                                    use form = new Form()
                                    form.Text <- "Update Book"
                                    form.FormBorderStyle <- FormBorderStyle.FixedDialog
                                    form.Width <- 300
                                    form.Height <- 200
                                    form.StartPosition <- FormStartPosition.CenterParent

                                    let titleTb = new TextBox(Text=book.Title, Location=Point(20,20), Width=200)
                                    let authorTb = new TextBox(Text=book.Author, Location=Point(20,60), Width=200)

                                    let saveBtn = new Button(Text="Save", Location=Point(60,100), Width=80, BackColor=Color.LightGreen)
                                    saveBtn.Click.Add(fun _ ->
                                        if not (String.IsNullOrWhiteSpace(titleTb.Text)) then book.Title <- titleTb.Text
                                        if not (String.IsNullOrWhiteSpace(authorTb.Text)) then book.Author <- authorTb.Text
                                        Database.updateBookInDb book
                                        loadCards()
                                        form.Close()
                                    )

                                    form.Controls.AddRange [| titleTb; authorTb; saveBtn |]
                                    form.ShowDialog() |> ignore
                                )
                                (fun book ->
                                    Database.deleteBookFromDb book
                                    loadCards()
                                )

                match b.Status with
                | Borrowed -> card.BackColor <- Color.LightCoral
                | Available -> card.BackColor <- Color.LightGreen

                cardsPanel.Controls.Add(card)

        loadCards()

        addButton.Click.Add(fun _ ->
            if String.IsNullOrWhiteSpace(titleTextBox.Text) || String.IsNullOrWhiteSpace(authorTextBox.Text) then
                MessageBox.Show("Both Title and Author are required!") |> ignore
            else
                Database.addBookToDb titleTextBox.Text authorTextBox.Text
                titleTextBox.Clear()
                authorTextBox.Clear()
                loadCards()
        )

        searchButton.Click.Add(fun _ ->
            let q = searchTextBox.Text.Trim().ToLower()
            cardsPanel.Controls.Clear()

            let books =
                Database.loadBooksFromDb()
                |> Seq.filter (fun b ->
                    b.Title.ToLower().Contains(q) ||
                    b.Author.ToLower().Contains(q)
                )

            for b in books do
                let card =
                    UIHelpers.createCardWithSelection b
                        (fun book ->
                            use form = new Form()
                            form.Text <- "Update Book"
                            form.FormBorderStyle <- FormBorderStyle.FixedDialog
                            form.Width <- 300
                            form.Height <- 200
                            form.StartPosition <- FormStartPosition.CenterParent

                            let titleTb = new TextBox(Text=book.Title, Location=Point(20,20), Width=200)
                            let authorTb = new TextBox(Text=book.Author, Location=Point(20,60), Width=200)

                            let saveBtn = new Button(Text="Save", Location=Point(60,100), Width=80, BackColor=Color.LightGreen)
                            saveBtn.Click.Add(fun _ ->
                                if not (String.IsNullOrWhiteSpace(titleTb.Text)) then book.Title <- titleTb.Text
                                if not (String.IsNullOrWhiteSpace(authorTb.Text)) then book.Author <- authorTb.Text
                                Database.updateBookInDb book
                                loadCards()
                                form.Close()
                            )

                            form.Controls.AddRange [| titleTb; authorTb; saveBtn |]
                            form.ShowDialog() |> ignore
                        )
                        (fun book ->
                            Database.deleteBookFromDb book
                            loadCards()
                        )

                match b.Status with
                | Borrowed -> card.BackColor <- Color.LightCoral
                | Available -> card.BackColor <- Color.LightGreen

                cardsPanel.Controls.Add(card)
        )


        showAllButton.Click.Add(fun _ -> loadCards())

        logoutButton.Click.Add(fun _ ->
            this.Hide()
            loginForm.Show()
        )

        this.Controls.AddRange [| topPanel; addPanel; cardsPanel |]
