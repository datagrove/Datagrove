
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class Test {
   public string name;
   public Func<ValueTask> exec;
   Test(string name, Func<ValueTask>  exec) {
      this.name = name;
      this.exec = exec;
   }


   public static List<string> shardedFilter(string filter, int size) {
            // execute each test in parallel. How do we capture the console output of multiple threads though? 
            var r = new List<string>();
            var tests = (from t in Test.allTests() 
                  where t.name.Contains(filter)
                  select t).ToList();
            // for each task give it a maximum amount of time and also catch and display task dumps. What if instead we created a special filter for dotnet test? Then dotnet test would do the whole batch for us. we could then use a seperate routine to restore the database.
            // should we simply invoke dotnet test on ourselves right here?
            // potential downside is we are creating TestResults for each batch.
           

            int i = 0;
            while (i < tests.Count()) {
               var s = new List<string>();
               for(var j=0; j<size && i<tests.Count(); j++,i++) {
                  s.Add("Name="+tests[i].name);
               }  
               r.Add(String.Join("|", s));
            }
            return r;
   }



   public static List<Test> allTests(){
      var a = new List<Assembly>{Assembly.GetExecutingAssembly()};
      var r = new List<Test>();
      // Find all the test methods and test classes.
           foreach (var prog in a)
            foreach (var t in prog.GetTypes())
            {
                var ca = (TestClassAttribute?)t.GetCustomAttribute(typeof(TestClassAttribute), false);
                if (ca == null) continue;
                foreach (var m in t.GetMethods())
                {
                    object[]? tc = m.GetCustomAttributes(typeof(TestMethodAttribute), false);
                    if (tc!=null && tc.Length > 0) {
                     r.Add(new Test(m.Name, async()=> {
                        await Task.CompletedTask;
                     } ));
                    }
                }
            }
      r = (from t in r orderby t.name select t).ToList();
      return r;
   }
}
