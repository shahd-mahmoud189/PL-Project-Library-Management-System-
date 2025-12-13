namespace LibraryUI

open System
open System.Data.SqlClient

module Database =

    let connectionString =
        "Server=localhost\\MSSQLSERVER01;Database=LibraryDB;Trusted_Connection=True;"

    let hashPassword (password:string) =
        use sha = System.Security.Cryptography.SHA256.Create()
        password
        |> System.Text.Encoding.UTF8.GetBytes
        |> sha.ComputeHash
        |> Array.map (fun b -> b.ToString("x2"))
        |> String.concat ""

    let addUser (username:string) (email:string) (password:string) (role:string) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        use cmd = new SqlCommand(
            "INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@u,@e,@p,@r)", conn)
        cmd.Parameters.AddWithValue("@u", username) |> ignore
        cmd.Parameters.AddWithValue("@e", email) |> ignore
        cmd.Parameters.AddWithValue("@p", hashPassword password) |> ignore
        cmd.Parameters.AddWithValue("@r", role) |> ignore
        cmd.ExecuteNonQuery() |> ignore

    let getUserByEmail (email:string) : User option =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        use cmd = new SqlCommand("SELECT Id, Username, Email, PasswordHash, Role FROM Users WHERE Email=@e", conn)
        cmd.Parameters.AddWithValue("@e", email) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then
            let id = reader.GetInt32(0)
            let uname = reader.GetString(1)
            let mail = reader.GetString(2)
            let phash = reader.GetString(3)
            let role = if reader.GetString(4) = "Admin" then Role.Admin else Role.User
            Some { Id=id; Username=uname; Email=mail; PasswordHash=phash; Role=role }
        else None

    let verifyLoginEmail (email:string) (password:string) : User option =
        match getUserByEmail email with
        | Some user when user.PasswordHash = hashPassword password -> Some user
        | _ -> None


    let loadBooksFromDb() =
        let books = new System.Collections.Generic.List<Book>()
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand("SELECT Id, Title, Author, Status FROM Books", conn)
        use reader = cmd.ExecuteReader()
        while reader.Read() do
            let status =
                match reader.GetString(3) with
                | "Available" -> BookStatus.Available
                | "Borrowed" -> BookStatus.Borrowed
                | _ -> BookStatus.Available
            books.Add(Book(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), status))
        books

    let addBookToDb title author =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand("INSERT INTO Books (Title, Author, Status) VALUES (@t,@a,'Available')", conn)
        cmd.Parameters.AddWithValue("@t", title) |> ignore
        cmd.Parameters.AddWithValue("@a", author) |> ignore
        cmd.ExecuteNonQuery() |> ignore

    let updateBookInDb (b:Book) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand("UPDATE Books SET Title=@t, Author=@a, Status=@s WHERE Id=@id", conn)
        cmd.Parameters.AddWithValue("@t", b.Title) |> ignore
        cmd.Parameters.AddWithValue("@a", b.Author) |> ignore
        cmd.Parameters.AddWithValue("@s",
            match b.Status with | Available -> "Available" | Borrowed -> "Borrowed") |> ignore
        cmd.Parameters.AddWithValue("@id", b.Id) |> ignore
        cmd.ExecuteNonQuery() |> ignore

    let deleteBookFromDb (b:Book) =
        use conn = new SqlConnection(connectionString)
        conn.Open()

        let cmdDelBorrowed = new SqlCommand("DELETE FROM BorrowedBooks WHERE BookId=@Id", conn)
        cmdDelBorrowed.Parameters.AddWithValue("@Id", b.Id) |> ignore
        cmdDelBorrowed.ExecuteNonQuery() |> ignore

        let cmd = new SqlCommand("DELETE FROM Books WHERE Id=@Id", conn)
        cmd.Parameters.AddWithValue("@Id", b.Id) |> ignore
        cmd.ExecuteNonQuery() |> ignore


    let getBorrowedCountByUser (userId:int) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand(
            "SELECT COUNT(*) FROM BorrowedBooks WHERE UserId=@u AND ReturnDate IS NULL", conn)
        cmd.Parameters.AddWithValue("@u", userId) |> ignore
        cmd.ExecuteScalar() :?> int

    let isBookBorrowedByUser (userId:int) (bookId:int) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand(
            "SELECT COUNT(*) FROM BorrowedBooks WHERE UserId=@u AND BookId=@b AND ReturnDate IS NULL", conn)
        cmd.Parameters.AddWithValue("@u", userId) |> ignore
        cmd.Parameters.AddWithValue("@b", bookId) |> ignore
        (cmd.ExecuteScalar() :?> int) > 0

    let borrowBook (userId:int) (bookId:int) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand(
            "INSERT INTO BorrowedBooks (UserId, BookId, BorrowDate) VALUES (@u,@b,GETDATE())", conn)
        cmd.Parameters.AddWithValue("@u", userId) |> ignore
        cmd.Parameters.AddWithValue("@b", bookId) |> ignore
        cmd.ExecuteNonQuery() |> ignore

    let returnBook (userId:int) (bookId:int) =
        use conn = new SqlConnection(connectionString)
        conn.Open()
        let cmd = new SqlCommand(
            "UPDATE BorrowedBooks SET ReturnDate=GETDATE() WHERE UserId=@u AND BookId=@b AND ReturnDate IS NULL", conn)
        cmd.Parameters.AddWithValue("@u", userId) |> ignore
        cmd.Parameters.AddWithValue("@b", bookId) |> ignore
        cmd.ExecuteNonQuery() |> ignore