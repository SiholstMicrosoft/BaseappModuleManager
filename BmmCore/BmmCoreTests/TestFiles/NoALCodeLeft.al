// Comment Start.
// !bmm-ignore
codeunit 50100 MyCodeunit
{
  trigger OnRun()
  begin
    
  end;
  
  // Middle Comment (should not appear after parsing).
  var
    myInt: Integer;
}
// Comment End.