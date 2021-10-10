namespace TableTypes
type Table()=
    member val userId = 0 with get, set
    member val tableName = "" with get, set
    member val tableHeaders: list<string>= [] with get, set
    member val tableDataTypes: list<string> = []  with get, set
    member val tableDatas = [[]] with get, set

type TableData()=
    member val userId = 0 with get, set
    member val tableName = "" with get, set
    member val tableHeaders: list<string>= [] with get, set
    member val tableDatas = [[]] with get, set