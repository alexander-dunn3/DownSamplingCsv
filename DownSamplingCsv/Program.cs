// get and validate csv file path
Console.WriteLine("Enter the full csv file path: ");
string? csvFilePath = Console.ReadLine();
string[] validExtensions = new string[] { ".csv" };
if (!File.Exists(csvFilePath))
{
  Console.WriteLine("The file doesn't exist");
  return;
}
string fileExtension = Path.GetExtension(csvFilePath).ToLowerInvariant();
if (!validExtensions.Contains(fileExtension))
{
  Console.WriteLine($"Sorry this program isn't able to handle {fileExtension} files yet.");
  return;
}

// get and validate the header rows to ignore
string contents = File.ReadAllText(csvFilePath);
string[] csvRows = contents.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
Console.WriteLine("How many header rows would you like to ignore? (Note: these rows are preserved in the new file created): ");
int ignore;
bool ignoreOutOfRange = false;
bool isANumber;
do
{
  isANumber = int.TryParse(Console.ReadLine(), out ignore);

  if (!isANumber)
    Console.WriteLine("Sorry I don't understand that number. Have another go...");
  else
  {
    ignoreOutOfRange = ignore >= csvRows.Length;

    if (ignoreOutOfRange)
      Console.WriteLine($"{ignore} row to ignore is too many for this file {csvFilePath} that has {csvRows.Length} rows. Have another go...");
  }
} while (!isANumber || ignoreOutOfRange);

// get and validate the downsample ratio
Console.WriteLine($"There are {csvRows.Length} rows found in the file. Enter the downsample ratio you'd like: ");
int downsampleRatio;
while (!int.TryParse(Console.ReadLine(), out downsampleRatio))
{
  Console.WriteLine("Sorry I don't understand that number. Have another go...");
}

// extract the header and the downsampled rows
ICollection<string> header = new List<string>();
ICollection<string> downSampledRows = new List<string>();
for (var i = 0; i < csvRows.Length; i++)
{
  string? row = csvRows[i];
  bool toBeIgnored = (i + 1) <= ignore;
  if (toBeIgnored)
    header.Add(row);
  else if (i % downsampleRatio == 0)
    downSampledRows.Add(row);
}

// ensure we have a unique filename
string? outputDirectory = Path.GetDirectoryName(csvFilePath);
if (outputDirectory == null) // for reasons unknown Path.GetDirectoryName can return null
                             // if containing directory is root so cater for this here
  outputDirectory = Path.GetPathRoot(csvFilePath);

if (string.IsNullOrEmpty(outputDirectory))
{
  Console.WriteLine($"It appears I can't handle the directory that {csvFilePath} is in. Please move it somewhere else and try again.");
  return;
}
string? originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFilePath);
string? outputPath;
bool outputFileExists;
int count = 0;
do
{
  string outputFileNameWithoutExtension = string.Join("_", new[] { originalFileNameWithoutExtension, count++.ToString() });
  outputPath = Path.Combine(outputDirectory, outputFileNameWithoutExtension + fileExtension);
  outputFileExists = File.Exists(outputPath);
} while (outputFileExists);

// write output file
string outputContents = string.Join(Environment.NewLine, header);
outputContents += string.Join(Environment.NewLine, downSampledRows);
Console.WriteLine($"Writing file {outputPath} to disk...");
File.WriteAllText(outputPath, outputContents);
Console.WriteLine("Done");
