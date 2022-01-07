/// <summary>
/// a simple regex commit linter example
/// https://www.conventionalcommits.org/en/v1.0.0/
/// https://github.com/angular/angular/blob/22b96b9/CONTRIBUTING.md#type
/// </summary>

using System.Text.RegularExpressions;
var pattern = @"^(?=.{1,90}$)(CSIOT-(\d+):\s)?(build|feat|ci|chore|docs|fix|perf|refactor|revert|style|test)(\([a-z-]+\))*(:)([-a-zA-Z0-9._ ])+(#\d+)*(?<![\.\s])$";

var file = File.ReadAllLines(Args[0]);
var msg = file[0];

if (Regex.IsMatch(msg, pattern))
   return 0;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("Invalid commit message");
Console.ResetColor();
Console.WriteLine("e.g: 'feat(scope): subject' or 'fix: subject', optionally you can prepend it with issue ID, like 'CSIOT-123: refactor(scope): subject'");
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("more info: https://www.conventionalcommits.org/en/v1.0.0/");

return 1;