namespace LibraryUI

type Role = 
    | Admin
    | User

type User = {
    Id: int
    Username: string
    PasswordHash: string
    Role: Role
    Email: string
}

type BookStatus =  
    | Available 
    | Borrowed 
 
type Book(id:int, title:string, author:string, status:BookStatus) = 
    member val Id = id with get, set 
    member val Title = title with get, set 
    member val Author = author with get, set 
    member val Status = status with get, set 
