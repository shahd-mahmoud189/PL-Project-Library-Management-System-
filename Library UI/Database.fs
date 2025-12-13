namespace LibraryUI
open System
open System.Data.SqlClient
open LibraryUI
module database = 
      
    let connectionString =
        "Server=localhost\\MSSQLSERVER01;Database=LibraryDB;Trusted_Connection=True;"
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
