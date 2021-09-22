tableextension 50100 "Item Ext Test" extends Item
{
    // Don't prefix extension fields.
    fields
    {
        field(1337; TestField1; Text[10])
        {

        }
        field(13337; "Test Field 2"; Text[10])
        {

        }
    }

    // Prefix global variables.
    var
        globalVariable1: Integer;
        "global Variable 2": Boolean;
        "Test Field 2": Text[10];

    // Prefix global variables.
    protected var
        protectedGlobalVariable1: Integer;
        "protected Global Variable 2": Boolean;
        "No.": Code[20];

    // Don't prefix public procedures.
    procedure Test1()
    begin
        // Don't prefix parent fields but prefix global variable.
        Rec."No." := "No.";

        // Don't prefix extension fields but prefix the global variable.
        TestField1 := Format(globalVariable1);

        // Prefix local procedures.
        "Test 2";


        with Rec do begin
            // Don't prefix extension fields.
            "Test Field 2" := "Test Field 2";

            // We cannot prefix this global variable as it might be from the parent object.
            "protected Global Variable 2" := true;
        end;
    end;

    // Prefix local procedures.
    local procedure "Test 2"()
    var
        Customer: Record Customer;
        globalVariable1: Integer;
    begin
        // Don't prefix local variable (this one is overriding the global variable).
        globalVariable1 := 0;

        // Don't prefix extension fields but prefix the global variable.
        Rec."Test Field 2" := "Test Field 2";

        // Prefix the global variable.
        Rec."protected Global Variable 2" := true;

        // Prefix the global variable.
        Customer."No." := "Test Field 2";
    end;
}
