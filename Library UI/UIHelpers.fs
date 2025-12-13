namespace LibraryUI

open System
open System.Windows.Forms
open System.Drawing
open LibraryUI.Database

module UIHelpers =

    let createCardWithSelection (b: Book) (onUpdate: Book -> unit) (onDelete: Book -> unit) =
        let card = new Panel(Width=500, Height=100, BorderStyle=BorderStyle.FixedSingle, Padding=Padding(5))
        card.BackColor <- Color.LightPink
        card.Margin <- Padding(10) 

        let titleLabel = new Label(Text = $"Title: {b.Title}", AutoSize=true)
        titleLabel.Location <- Point(10, 10)

        let authorLabel = new Label(Text = $"Author: {b.Author}", AutoSize=true)
        authorLabel.Location <- Point(10, 35)

        let statusLabel = new Label(Text = $"Status: {b.Status}", AutoSize=true)
        statusLabel.Location <- Point(300, 10)
        statusLabel.ForeColor <- if b.Status = Borrowed then Color.Red else Color.Green

        let updateBtn = new Button(Text="Update", Width=70, Height=28)
        updateBtn.Location <- Point(300, 55)
        updateBtn.BackColor <- Color.LightYellow
        updateBtn.Click.Add(fun _ -> onUpdate b)

        let deleteBtn = new Button(Text="Delete", Width=70, Height=28)
        deleteBtn.Location <- Point(380, 55)
        deleteBtn.BackColor <- Color.LightCoral
        deleteBtn.Click.Add(fun _ -> onDelete b)

        card.Controls.AddRange [| titleLabel; authorLabel; statusLabel; updateBtn; deleteBtn |]
        card.Tag <- b
        card

    let reloadBooks (flowPanel:FlowLayoutPanel) (onUpdate: Book -> unit) (onDelete: Book -> unit) =
        flowPanel.Controls.Clear()
        for b in Database.loadBooksFromDb() |> Seq.toList do
            flowPanel.Controls.Add(createCardWithSelection b onUpdate onDelete)

    let addNewBook (flowPanel:FlowLayoutPanel) (titleTextBox:TextBox) (authorTextBox:TextBox)
                  (onUpdate: Book -> unit) (onDelete: Book -> unit) =
        if String.IsNullOrWhiteSpace(titleTextBox.Text) || String.IsNullOrWhiteSpace(authorTextBox.Text) then
            MessageBox.Show("Both Title and Author are required!") |> ignore
        else
            Database.addBookToDb titleTextBox.Text authorTextBox.Text
            reloadBooks flowPanel onUpdate onDelete
            titleTextBox.Clear()
            authorTextBox.Clear()

    let borrowBookCard (selectedCard:Panel) (flowPanel:FlowLayoutPanel) =
        if selectedCard <> null && selectedCard.Tag <> null then
            let b = selectedCard.Tag :?> Book
            if b.Status = Available then
                b.Status <- Borrowed
                Database.updateBookInDb b
                reloadBooks flowPanel (fun _ -> ()) (fun _ -> ())

    let returnBookCard (selectedCard:Panel) (flowPanel:FlowLayoutPanel) =
        if selectedCard <> null && selectedCard.Tag <> null then
            let b = selectedCard.Tag :?> Book
            if b.Status = Borrowed then
                b.Status <- Available
                Database.updateBookInDb b
                reloadBooks flowPanel (fun _ -> ()) (fun _ -> ())

    let searchBook (flowPanel:FlowLayoutPanel) (title:string) (onSelect: Panel -> unit) =
        flowPanel.Controls.Clear()
        let books =
            if String.IsNullOrWhiteSpace(title) then Database.loadBooksFromDb() |> Seq.toList
            else Database.loadBooksFromDb() |> Seq.filter (fun b -> b.Title.ToLower().Contains(title.ToLower())) |> Seq.toList
        for b in books do
            flowPanel.Controls.Add(createCardWithSelection b (fun _ -> ()) (fun _ -> ()))
