using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace SeleniumDocs.GettingStarted;

public static class TestTheWebPage
{
  public static string PageURL = "";

  public static string LookFor = "";

  public static string TagName = "div";
  public static int Main( string[] args )
  {

    // --- CLI parsing --------------------------------------------------------
    // Podporované tvary:
    //   -u https://example.com    --url https://example.com    --url=https://example.com
    //   -l "řetězec"              --lookfor "řetězec"          --lookfor=řetězec
    //   -t body                   --tag body                    --tag=body
    ApplyCliArgs( args );

    int returnCode = 2; // 2 = error, text nenalezen; 1 = error, parametry; 0 = ok

    FirefoxDriver? driver = null;

    try
    {

      var service = FirefoxDriverService.CreateDefaultService();  
      service.LogLevel = FirefoxDriverLogLevel.Debug;
      service.LogPath = "geckodriver.log";

      var options = new FirefoxOptions();
      options.AddArgument("-headless");

      driver = new FirefoxDriver( service, options );
      driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds( 500 );

      driver.Navigate().GoToUrl( PageURL );

      IList<IWebElement> elements = driver.FindElements( By.TagName( TagName ) );
      foreach(IWebElement e in elements)
      {
        System.Console.WriteLine( e.Text );

        if (e.Text.Contains( LookFor ))
        {
          System.Console.WriteLine( "\nFound the text!\n" );
          returnCode = 0; // 0 = ok, text nalezen
          break;
        }
      }

    }
    finally
    {
      if(driver != null)
        driver.Quit();
    }

    if(returnCode == 2)
    {
      System.Console.WriteLine( $"\nText '{LookFor}' not found in tag '{TagName}' on page '{PageURL}'." );
    }

    return returnCode;

  }

  private static void ApplyCliArgs( string[] args )
  {
    if(args == null || args.Length == 0)
    {
      PrintUsage();
      Environment.Exit( 1 );
    }

    // mapování aliasů
    string? url = GetOption( args, "url", "u" );
    if(!string.IsNullOrWhiteSpace( url )) PageURL = url!;

    string? lookFor = GetOption( args, "lookfor", "l" );
    if(!string.IsNullOrWhiteSpace( lookFor )) LookFor = lookFor!;

    string? tag = GetOption( args, "tag", "t" );
    if(!string.IsNullOrWhiteSpace( tag )) TagName = tag!;

    if(HasFlag( args, "help", "h", "?" ))
    {
      PrintUsage();
      Environment.Exit( 1 );
    }
  }

  // Vrátí hodnotu pro --name=value | --name value | -n value
  private static string? GetOption( string[] args, string longName, string shortName )
  {
    // --name=value
    var pref = $"--{longName}=";
    var withEq = args.FirstOrDefault( a => a.StartsWith( pref, StringComparison.OrdinalIgnoreCase ) );
    if(withEq != null) return withEq.Substring( pref.Length );

    // --name value
    for(int i = 0; i < args.Length - 1; i++)
      if(string.Equals( args[i], $"--{longName}", StringComparison.OrdinalIgnoreCase ))
        return args[i + 1];

    // -n value
    for(int i = 0; i < args.Length - 1; i++)
      if(string.Equals( args[i], $"-{shortName}", StringComparison.OrdinalIgnoreCase ))
        return args[i + 1];

    return null;
  }

  private static bool HasFlag( string[] args, params string[] names )
    => args.Any( a => names.Any( n =>
         string.Equals( a, $"--{n}", StringComparison.OrdinalIgnoreCase ) ||
         string.Equals( a, $"-{n}", StringComparison.OrdinalIgnoreCase ) ) );

  private static void PrintUsage()
  {
    Console.WriteLine(
@"Usage:
  dotnet <dll> [options]

Options:
  -u, --url <URL>           Adresa testované stránky (PageURL)
  -l, --lookfor <TEXT>      Hledaný text (LookFor)
  -t, --tag <TAGNAME>       HTML tag, ve kterém hledat (TagName), např. body|div|p
  -h, --help                Zobrazí tuto nápovědu

Příklady:
  dotnet app.dll --url https://example.com --lookfor ""Hurá!"" --tag body
  dotnet app.dll -u https://example.com -l ""Welcome"" -t div" );
  }

}

