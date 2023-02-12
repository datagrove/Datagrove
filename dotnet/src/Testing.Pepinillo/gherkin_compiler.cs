// datagrove gherkin compiler
namespace Datagrove.Testing.Pepinillo;
using System.Text.RegularExpressions;
using Gherkin;
using System.Reflection;
using System.Text;
using Gherkin.Ast;
using System.IO;

using Compiled = System.Action<string, string, string>;
using System.CodeDom.Compiler;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Text.Json;
using static SimpleExec.Command;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

public class FileUtil
{
    static public string findFirstFile(string dir, string pattern)
    {
        var matcher = new Matcher();
        matcher.AddInclude(pattern);
        var di = new DirectoryInfo(dir);
        var result = matcher.Execute(new DirectoryInfoWrapper(di));
        if (result.HasMatches)
        {
            foreach (var f in result.Files)
            {
                if (f.Path.EndsWith(".csproj"))
                {
                    return f.Path;
                }
            }
        }
        return "";
    }

    public static string projectPath(string dir)
    {
        var csproj = FileUtil.findFirstFile(dir, "*.csproj");
        return csproj == null ? "" : $"{dir}/{csproj}";
    }
    public static Assembly? assemblyFromDirectory(string dir)
    {
        var csproj = FileUtil.findFirstFile(dir, "*.csproj");
        if (csproj=="") {
            throw new Exception("No project found");
        }
        var cmd = $"build \"{dir}/{csproj}\"";
        SimpleExec.Command.Run("dotnet", cmd);
        var fn = Path.GetFileNameWithoutExtension(csproj);
        var asm = Assembly.LoadFrom($"{dir}/bin/Debug/net7.0/{fn}.dll");
        return asm;
    }
}


// reuse builder so not reloading assembly?
// list of assemblies instead of one?
public class Pepin
{
    public static async Task build(string path)
    {
        var amaybe = FileUtil.assemblyFromDirectory(path);
        if (amaybe == null)
        {
            Console.WriteLine("no assembly found");
            return;
        }
        var a = new Assembly[] { amaybe };

        PepinilloConfig? s = null;

        var config = path + "/pepin.config.json";
        if (File.Exists(config))
        {
            try
            {
                using FileStream stream = File.OpenRead(config);
                s = await JsonSerializer.DeserializeAsync<PepinilloConfig>(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        // create the compiled projects
        string? outdir = null; // todo!! allow on command line, also pick one project from command line
        if (s == null)
        {
            // no config, use defaults
            new BuildGherkin(a).config(path, outdir, "pepinillo").build();
        }
        else
        {
            if (s.project == null || s.project.Count() == 0)
            {
                new BuildGherkin(a).config(path, outdir, "pepinillo", s).build();
            }
            else
            {
                foreach (var e in s.project)
                {
                    new BuildGherkin(a).config(path, outdir, s, e.Key, e.Value).build();
                }
            }
        }



    }

}

// each step is part of a class that will get initialized
// [Scope("gherkin_tag")]
// potentially add gherkin tags based on folder hierarchy
// naming base on first tag is bad idea?

// these are things we need to invoke at compile time to transform the string into a bool or an int or something. 
public class TransformArg
{

    Regex regex;
    MethodInfo m;
    Object obj;
    public TransformArg(String s, Object o, MethodInfo m)
    {
        this.regex = new Regex(s);
        this.m = m;
        this.obj = o;
    }
    public bool tryTransform(string s, Type t, out string r)
    {
        // if the string matches, then we need to pass it to the method then ToString() it's result.
        r = "";
        try
        {
            if (regex.IsMatch(s))
            {
                // m.ReturnType==t && 
                var o = m.Invoke(obj, new object[] { s });
                if (o is bool ob)
                {
                    r = ob ? "true" : "false";
                }
                else
                    r = o?.ToString() ?? "";
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }
}
public class StepClass
{
    public string name;
    public string[] constructorArgs;
    public StepClass(string name, string[] constructorArgs)
    {
        this.name = name;
        this.constructorArgs = constructorArgs;
    }
}
public class Namer
{
    Dictionary<string, int> mname = new Dictionary<string, int>();

    public string name(string sname)
    {
        int n = 0;
        if (mname.TryGetValue(sname, out n))
        {
            n++;
        }
        mname[sname] = n;
        if (n != 0)
            sname += $"{n}";
        return sname;
    }
}
public class MissingStep
{
    public Step step;
    public string name;

    public MissingStep(Step step)
    {
        this.step = step;
        this.name = new string(step.Text.Trim().Select(e => char.IsAsciiLetterOrDigit(e) ? e : '_').ToArray());
    }
    public string stepCall() => $"new MissingStep().{name}();";
    //     private static string ToLiteral(string input)
    // {
    //     using (var writer = new StringWriter())
    //     {
    //         using (var provider = CodeDomProvider.CreateProvider("CSharp"))
    //         {
    //             provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
    //             return writer.ToString();
    //         }
    //     }
    // }
    public void writeTo(IndentedTextWriter s)
    {
        var tx = step.Text.Trim();
        var fmtx = tx.Replace("\"", "\\\"");

        var param = "";
        var key = step.Keyword.Trim();
        if (key == "And") key = "When";
        s.WriteLine("");
        s.WriteLine($"[{key}(\"{fmtx}\")]");
        s.WriteLine($"public void {name}({param}) {{");
        s.Indent++;
        s.WriteLine("throw new NotImplementedException();");
        s.Indent--;
        s.WriteLine("}");
    }
}
// first create all the dependencies asynchronously
// next initalize the steps
// do the background steps
// finally do the scenario steps
public class GherkinCompiler
{
    // perhaps we can make an initial pass that generates missing methods?
    // why not one pass though?

    public void writeMissing(string path)
    {
        var baseTextWriter = new System.IO.StringWriter();
        var methods = new IndentedTextWriter(baseTextWriter, "    ");
        methods.WriteLine(@"//Do not edit this generated file!. You can modify these step definitions to fill in the missing steps and copy them into your step project");
        methods.WriteLine("using TechTalk.SpecFlow;");
        methods.WriteLine("public class MissingStep {");
        methods.Indent++;
        foreach (var e in stubSteps)
        {
            e.Value.writeTo(methods);
        }
        methods.Indent--;
        methods.WriteLine("}");
        File.WriteAllText(path, baseTextWriter.ToString());
    }


    public Dictionary<string, MissingStep> stubSteps = new();


    public void compile(Compiled compiled, string file, StepSet ss, Namer nm, BuildGherkin bg, GherkinDocumentation docm)
    {
        var StepState = bg.scenarioState;
        var parser = new Parser();
        try
        {
            var doc = parser.Parse(file);
            var x = new StringBuilder();
            // Creates a TextWriter to use as the base output writer.
            var baseTextWriter = new System.IO.StringWriter();
            var methods = new IndentedTextWriter(baseTextWriter, "    ");
            methods.Indent = 1;

            var bgb = new System.IO.StringWriter();
            var bgw = new IndentedTextWriter(bgb, "    ");
            bgw.Indent++;

            var tags = String.Join("", doc.Feature.Tags.Select((e) => $"[TestCategory(\"{e.Name.Substring(1)}\")]\n"));

            var name = symbol(doc.Feature.Name);
            name = nm.name(name);
            docm.addFeature(file, name);
            var comments = doc.Comments;
            var children = doc.Feature.Children;
            var nmr = new Namer();

            // each step has a class and we need to collect them to add them as members to the class.
            var stepClass = new Dictionary<string, StepClass>();
            var appendStep = (Step step, string text, IndentedTextWriter tw) =>
            {
                var cstep = ss.compile(step, text, (string w) => compiled(file, "", w));
                if (cstep == null)
                {
                    var error = $"\n## Not found\n{text}\n";
                    compiled(file, "", error);
                    var st = new MissingStep(step);
                    if (!stubSteps.ContainsKey(step.Text))
                        stubSteps.Add(step.Text, st);
                    tw.WriteLine(st.stepCall());
                    // for debugging
                    var cstep2 = ss.compile(step, text, (string w) => compiled(file, "", w));
                }
                else
                {
                    docm.addStep(cstep);
                    tw.WriteLine(cstep.csharp);
                    var cname = cstep.step.classType.FullName ?? cstep.step.classType.Name;
                    if (!stepClass.ContainsKey(cname))
                    {
                        var args = new string[] { };
                        ConstructorInfo[] ci = cstep.step.classType.GetConstructors();
                        if (ci.Length > 0)
                        {
                            var p = ci[0].GetParameters();
                            args = p.Select((e) => e.Name ?? "").ToArray();
                        }
                        StepClass scx = new StepClass(cname, args ?? new string[] { });

                        stepClass.Add(scx.name, scx);
                        // backgrounds don't initialize, they only use the members.

                        //methods.WriteLine($"shot(\"{Uri.EscapeDataString(text.Replace(' ', '_'))}\");");
                    }
                }
            };
            foreach (var ch in doc.Feature.Children)
            {

                if (ch is Rule rule)
                {
                    // rules are a way to group methods and give them common tags
                }
                else if (ch is Background background)
                {
                    // we need to gather the steps, treat as a subroutine or macro expansion? if it's a subroutine, how do we share the context? It's class wide anyway.
                    // there are no examples in background steps.            
                    foreach (var st in background.Steps)
                    {
                        appendStep(st, st.Text, bgw);
                    }
                }
                else if (ch is Scenario scenario)
                {
                    bool ignore = false;
                    foreach (var tg in scenario.Tags)
                    {
                        if (tg.Name == "@Ignore" || tg.Name == "@Ignored")
                            ignore = true;
                        else
                            methods.WriteLine($"[TestCategory(\"{tg.Name.Substring(1)}\")]");
                    }
                    // we can't generate ignore methods, because they typically don't have steps.
                    if (ignore) continue;

                    var sname = scenario.Name;
                    // maybe here we should allow a @name:GoodName?
                    sname = symbol(sname);
                    sname = nmr.name(sname);
                    if (sname == name)
                    {
                        // can't use the class name/Feature name
                        sname += "_";
                    }

                    // collect the classes needed for this test.
                    var classes = new Dictionary<string, string>();

                    docm.addScenario(sname);

                    // !! we need to initialize the classes here
                    // we need to initialize any that are used in the background as well
                    // we need to call background if it exists.

                    var exn = 0;
                    // we should have a better way of doing this, most things don't need macro expansion.
                    var oneExample = (Dictionary<string, string> ed) =>
                    {
                        var suffix = exn == 0 ? "" : $"__{exn}";
                        methods.WriteLine(bg.TestMethod());
                        methods.WriteLine($"public async Task {sname}{suffix}()");
                        methods.WriteLine("{");
                       // methods.WriteLine("try {");
                        methods.Indent++;
                        methods.WriteLine($"await using (var context = await {StepState}.create(TestContext!)){{");

                        methods.Indent++;
                        methods.WriteLine("var step = new Steps(context);");
                        methods.WriteLine($"await step.background();");
                        foreach (var st in scenario.Steps)
                        {
                            var tx = st.Text;
                            foreach (var e in ed)
                            {
                                tx = tx.Replace("<" + e.Key + ">", e.Value);
                            }
                            appendStep(st, tx, methods);
                        }
                        methods.Indent--;
                        methods.WriteLine("}"); // end using
                        methods.Indent--;
                        // methods.WriteLine("} catch(Exception e){");
                        // methods.Indent++;
                        // methods.WriteLine("throw e;");
                        // methods.Indent--;
                        // methods.Write("}");
                        // methods.Indent--;
                        methods.WriteLine("}");
                        exn++;
                    };

                    // we can have multiple Examples? how does that work?
                    var ed = new Dictionary<string, string>();
                    int cnt = scenario.Examples.Count();
                    if (cnt == 0)
                    {
                        oneExample(ed);
                    }
                    else if (cnt > 0)
                    {
                        if (cnt > 1)
                        {
                            Console.Write("");
                        }
                        var tbl = scenario.Examples.First()!;
                        string[] header = tbl.TableHeader.Cells.Select(e => e.Value).ToArray();
                        var d = new List<string>();
                        foreach (var k in tbl.TableBody)
                        {
                            ed.Clear();
                            int ik = 0;
                            foreach (var cl in k.Cells)
                            {
                                ed.Add(header[ik++], cl.Value);
                            }
                            oneExample(ed);
                        }

                    }


                }
            }

            // we have to match the constructor to the StepState
            // we can substitute all the constructor arguments x[i] with context.x[i]

            var member = "";
            var featureConstructor = "";
            foreach (var kv in stepClass)
            {
                // substitute short names
                var cn = kv.Key;
                StepClass sc = kv.Value;

                var nm3 = ss.usingNamespace[cn];
                var nmx = nm3.Substring(0, 1).ToLower() + nm3.Substring(1);
                member += $"        internal {cn} {nmx};\n";

                var args = sc.constructorArgs.Select(e => $"context.{e}").ToList();
                var args2 = String.Join(',', args);
                featureConstructor += $"            {nmx} = new {cn}({args2});\n";

                // if ("inventoryReceiptsStepDef" != nmx)
                // {
                //     featureConstructor += $"            {nmx} = new {cn}(context.driver,context.context);\n";
                // }
                // else
                // {
                //     featureConstructor += $"            {nmx} = new {cn}(context.driver,context.context,context.scenario);\n";
                // }
            }



            var stepDef = @$"
    public class Steps {{
{member}
        public Steps({StepState} context)
        {{
{featureConstructor}
        }}

        public async Task background()
        {{
            var step=this;
            {bgb.ToString()}
            await Task.CompletedTask;
        }}
    }}
";

            var debugInfo = "";
            var category = bg.tag == "" ? "" : $"[TestCategory(\"{bg.tag}\")]";
            var result = $@"
// {file}
[TestClass()]
{category}
{tags}public class {name} {bg.baseTest}
{{
    public TestContext? TestContext {{ get; set; }}
    {stepDef}
    {baseTextWriter.ToString()}
    {debugInfo}
}}
";
            compiled(file, result, "");
        }
        catch (Exception e)
        {
            compiled(file, "", e.ToString());
        }
    }


    public StepSet ss;

    public GherkinCompiler(string[] stepSpace, Assembly[] prog)
    {
        if (prog.Count() == 0)
        {
            prog = new Assembly[] { System.Reflection.Assembly.GetExecutingAssembly() };
        }
        ss = new StepSet(prog, stepSpace);
    }

    // we should also give this a set of assemblies
    // compiled(path,result,error)
    public GherkinDocumentation compile(Compiled compiled, string[] files, BuildGherkin bg)
    {
        var doc = new GherkinDocumentation(bg, files);
        var nmr = new Namer();
        for (var j = 0; j < files.Count(); j++)
        {
            compile(compiled, files[j], ss, nmr, bg, doc);
        }
        return doc;
    }

    public string[] findFeatures(string indir, string[] dir)
    {
        var r = new List<string>();
        foreach (var o in dir)
        {
            var op = Path.Join(indir, o);
            r = r.Concat(Directory.GetFiles(op,
                            "*.feature",
                            SearchOption.AllDirectories)).ToList();
        }
        return r.ToArray();
    }


    static public string symbol(string x)
    {
        var sb = new StringBuilder();
        foreach (var c in x)
        {
            if (c >= 48 && c <= 57 || c >= 65 && c <= 90 || c >= 97 && c <= 122)
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }

}

enum TestSdk
{
    mstest,
    nunit,
    xunit
}
// Gherkin doesn't natively understand namespaces, so we need to add some additional information about what types are useful for a given feature folder.
public class BuildGherkin
{
    TestSdk sdk = TestSdk.mstest;

    public string TestMethod()
    {
        switch (sdk)
        {

            default:
                return "[TestMethod()]";
        }
    }
    public Assembly[] assembly;
    public string[] stepSpace = new string[] { };
    public string[] featureFolder = new string[] { };


    public string baseTest = "";

    public BuildGherkin(Assembly[] assembly)
    {
        this.assembly = assembly;
    }

    public string tag = "";
    public string root { get; set; } = ".";
    public string outdir { get; set; } = "";

    PepinilloConfig? cfg;
    string outputStem = "";
    string projectName = "";
    string inCsproj = "";

    public string scenarioState { get; set; } = "";
    public string indir { get; set; } = "";

    public BuildGherkin config(string indir, string? outdir, string name, PepinilloConfig? cfg = null)
    {
        this.indir = indir;
        this.scenarioState = cfg?.scenarioState ?? "ScenarioState";
        this.tag = name;
        this.root = root;
        this.cfg = cfg;
        if (outdir == null)
        {
            var stem = Path.GetFileName(indir);
            outdir = $"{indir}/../{stem}.{name}";
        }
        this.outdir = outdir;

        // we should make this relative to the project
        this.inCsproj = FileUtil.projectPath(indir);

        this.baseTest = cfg?.baseTest ?? "";
        this.scenarioState = cfg?.scenarioState ?? "ScenarioState";
        if (baseTest != "")
        {
            baseTest = ": " + baseTest;
        }
        // this supports the case of arbitrary output directories
        projectName = outdir.Split(Path.DirectorySeparatorChar).Last() + ".csproj";
        this.outputStem = this.outdir + "/" + this.tag;
        return this;
    }
    public BuildGherkin config(string indir, string? outdir, PepinilloConfig cfg, string name, FeatureSpace config)
    {
        this.config(indir, outdir, name, cfg);
        // limit the features and steps for this space
        stepSpace = config.steps.ToArray();
        featureFolder = config.features.ToArray();
        return this;
    }

    public BuildGherkin build()
    {
        Directory.CreateDirectory(outdir);


        var csproj = $"""
    <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net7.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>

        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="MSTest.TestFramework" Version="3.0.2" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.4.1" />
        <PackageReference Include="MSTest.TestAdapter" Version="3.0.2" />
        <PackageReference Include="coverlet.collector" Version="3.1.2" />
        <PackageReference Include="SpecFlow" Version="4.0.16-beta" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="{this.inCsproj}" />
        <ProjectReference Include="/Users/jim/dev/datagrove/testing/dotnet/src/Testing.MSTest/Testing.MSTest.csproj" />
    </ItemGroup>

    </Project>
    """;
        if (false)
        {
            // delete all the files and create a new project
            new DirectoryInfo(outdir).GetFileSystemInfos().ToList().ForEach(x =>
                    {
                        if (x is DirectoryInfo di)
                            di.Delete(true);
                        else
                            x.Delete();
                    });
            File.WriteAllText(outdir + "/readme.md", "This is a generated project. Do not edit it directly. Instead, edit the source project and recompile.");
            File.WriteAllText(outdir + "/" + projectName, csproj);
        }

        if (featureFolder.Count() == 0)
        {
            featureFolder = new string[] { root };
        }
        var sb = new StringBuilder();
        var log = new StringBuilder();
        var errors = new HashSet<string>();
        var compiled = (string f, string r, string e) =>
        {
            if (e != "" && !errors.Contains(e))
            {
                log.Append(e);
                errors.Add(e);
            }
            sb.Append(r);
        };

        var c = new GherkinCompiler(stepSpace, assembly);
        var fl = c.findFeatures(indir, featureFolder);
        Console.Write($"{fl.Count()} feature files.");
        var doc = c.compile(compiled, fl, this);

        // not needed because we use fully qualified
        // + c.ss.usingText() 
        var stem = Path.GetFileName(root);
        File.WriteAllText(outputStem + ".cs", $@"// File generated by Pepinillo, If you edit this file, rename it and/or delete the .feature file that generates it.

#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
" + sb.ToString());

        // here we add a bunch of debug information the markdown loag.
        log.Append(c.ss.text());
        File.WriteAllText(outputStem + ".md", log.ToString());

        File.WriteAllText(outputStem + ".json", doc.json());
        var mpath = outdir + "/" + tag + (tag == "" ? "" : ".") + "MissingStep.cs";
        if (c.stubSteps.Count() > 0)
        {
            c.writeMissing(mpath);
        }
        else
        {
            File.Delete(mpath);
        }
        return this;
    }

}
