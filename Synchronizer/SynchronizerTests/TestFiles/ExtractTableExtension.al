tableextension 50100 "Item Ext Test" extends Item
{
    fields
    {
        field(1337; "Test Field 1"; Text[10])
        {

        }
        field(13337; "Test Field 2"; Text[10])
        {

        }
    }

    var
        globalVariable1: Integer;
        globalVariable2: Boolean;

    procedure Test1()
    begin
    end;

    procedure Test2()
    begin
    end;
}