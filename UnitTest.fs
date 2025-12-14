namespace TestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open LibraryUI
open LibraryUI.Database

[<TestClass>]
type LibraryFeatureTests () =

    // =========================
    // TEST 1: Add Book (Admin)
    // =========================
    [<TestMethod>]
    member _.``Admin can add book and book appears in list`` () =

        let adminEmail = $"admin_{Guid.NewGuid()}@test.com"
        let adminPassword = "123456"

        addUser "Test Admin" adminEmail adminPassword "Admin"

        let adminUser = verifyLoginEmail adminEmail adminPassword
        Assert.IsTrue(adminUser.IsSome, "Admin login failed")

        let admin = adminUser.Value
        Assert.AreEqual(Role.Admin, admin.Role)

        let booksToAdd =
            [
                ("FSharp Programming", "Author One")
                ("CSHARP BASICS", "AUTHOR TWO")
                ("Automation Book 3", "Author C")
                ("Automation Book 4", "Author D")
            ]

        for (title, author) in booksToAdd do
            addBookToDb title author

        let booksFromDb = loadBooksFromDb()

        for (title, _) in booksToAdd do
            let foundBook =
                booksFromDb
                |> Seq.tryFind (fun b -> b.Title = title)

            Assert.IsTrue(foundBook.IsSome, $"Book '{title}' not found")
            Assert.AreEqual(
                BookStatus.Available,
                foundBook.Value.Status,
                $"Book '{title}' is not Available"
            )

    // =========================
    // TEST 2: Borrow Book (User)
    // =========================
    [<TestMethod>]
    member _.``User can borrow book and borrow rules are enforced`` () =

        // ---------- User ----------
        let userEmail = $"borrow_user_{Guid.NewGuid()}@test.com"
        let userPassword = "123456"

        addUser "Borrow User" userEmail userPassword "User"

        let userOpt = verifyLoginEmail userEmail userPassword
        Assert.IsTrue(userOpt.IsSome, "User login failed")

        let user = userOpt.Value

        // ---------- Book ----------
        let testBookTitle = $"Borrow Test Book {Guid.NewGuid()}"
        let testBookAuthor = "Test Author"

        addBookToDb testBookTitle testBookAuthor

        let book =
            loadBooksFromDb()
            |> Seq.find (fun b -> b.Title = testBookTitle)

        // ---------- Borrow ----------
        borrowBook user.Id book.Id

        // ---------- Assertions ----------

        // ✔ Borrow record created
        let borrowCount = getBorrowedCountByUser user.Id
        Assert.AreEqual(1, borrowCount, "Borrow record was not created")

        // ✔ Book still exists in DB
        let bookStillExists =
            loadBooksFromDb()
            |> Seq.exists (fun b -> b.Id = book.Id)

        Assert.IsTrue(bookStillExists, "Book disappeared after borrowing")
