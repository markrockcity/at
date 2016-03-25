//CSharpScript (csi.exe)

#r "bin\Debug\At.exe"
using At;
var x = At.AtSyntaxTree.ParseText("@y<>; @class<>  : y<> {@P<>}");
var root = x.GetRoot();
var c = AtCompilation.Create(new[] {x});
var s = new MemoryStream();
var r = c.Emit(s);
var srcs = r.ConvertedSources().ToArray();
var src = srcs[0];