using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransformVott2CNTKDataset
{
    class Program
    {
        private static List<string> tags = new List<string>() { "__background__" };
        private static Encoding utf8WithoutBom = new UTF8Encoding(false);

        static void Main(string[] args)
        {
            ProcessAsync();
            Console.WriteLine("Done");
        }

        private static void ProcessAsync()
        {
            string baseDir = Environment.CurrentDirectory;
            string[] folders = new string[] { "positive", "testImages" };
            string[] output = new string[] { "train", "test" };
            for (int i = 0; i < folders.Length; i++)
            {
                List<string> img_file_txt = new List<string>();
                List<string> roi_file_txt = new List<string>();
                string folder = folders[i];
                var imageFiles = Directory.EnumerateFiles(Path.Combine(baseDir, folder), "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).OrderBy(s => Path.GetFileName(s), StringComparer.Ordinal);
                for (int j = 0; j < imageFiles.Count(); j++)
                {
                    string file = imageFiles.ElementAt(j);
                    string filename = Path.GetFileName(file);
                    string currentFilename = Path.Combine(baseDir, folder, Path.GetFileNameWithoutExtension(file));
                    string[] labels = File.ReadAllLines(currentFilename + ".bboxes.labels.tsv");
                    string[] coordinates = File.ReadAllLines(currentFilename + ".bboxes.tsv");

                    if (labels.Length != coordinates.Length)
                    {
                        throw new ArgumentException($"The labels and coordinates of {file} are not equal size!");
                    }

                    img_file_txt.Add($"{j}\t{folder}/{filename}\t0");
                    StringBuilder roiText = new StringBuilder();
                    roiText.Append(j);
                    roiText.Append(" |roiAndLabel ");
                    for (int k = 0; k < labels.Length; k++)
                    {
                        if (!tags.Contains(labels[k]))
                        {
                            tags.Add(labels[k]);
                        }
                        int tagIndex = tags.IndexOf(labels[k]);
                        roiText.Append(string.Join(" ", coordinates[k].Split('\t').Select(c => Decimal.Parse(c).ToString("0.0"))));
                        roiText.Append(" ");
                        roiText.Append(tagIndex.ToString("0.0"));
                        if (k != labels.Length - 1)
                        {
                            roiText.Append(" ");
                        }
                    }
                    roi_file_txt.Add(roiText.ToString());
                }
                File.WriteAllLines(Path.Combine(baseDir, output[i] + "_img_file.txt"), img_file_txt, utf8WithoutBom);
                File.WriteAllLines(Path.Combine(baseDir, output[i] + "_roi_file.txt"), roi_file_txt, utf8WithoutBom);
            }
            File.WriteAllLines(Path.Combine(baseDir, "class_map.txt"), tags.Select((t, index) => t + "\t" + index), utf8WithoutBom);
        }
    }
}
