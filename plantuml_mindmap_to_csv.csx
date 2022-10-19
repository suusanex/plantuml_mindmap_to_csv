//PlantUMLのMindMap（OrgMode）を、各枝を1行としたCSVファイルへ変換するスクリプト。
//ツリー構造でいったんブレークダウンしたものを、表形式にして各要素の値を合計する必要がある、という用途（WBS作成など）を想定している。
//注意：MindMapの記載に「,」が含まれる場合は、「、」へ置換する

using System.Text.RegularExpressions;

#region 入力パラメータ

/// <summary>
/// 入力ファイルのパス。このファイルで最初に出てきた@startmindmapを処理対象とする。
/// </summary>
var inputFilePath = @"<InputFilePath>";

/// <summary>
/// 出力するCSVファイルのパス。
/// </summary>
var outputFilePath = @"<OutputFilePath>.csv";

#endregion

#region 処理

void TraceLog(string msg){
    Console.WriteLine(msg);
}

//用途から見てさほど巨大なデータにはならないので、全てメモリ上で処理して最後にファイル出力する
List<string> outputBuf = new();

bool isStartedMindMap = false;
int lastDepth = 0;
List<string> lastCsvLine = new();

using(var file = new StreamReader(inputFilePath)){

    while(true){

        var line = file.ReadLine();
        if(line == null) break;

        //最初の@startmindmapから@endmindmapまでの範囲を処理対象とする
        if(!isStartedMindMap)
        {
            if(line.Contains("@startmindmap")){
                isStartedMindMap = true;
                TraceLog("Start Mindmap");
            }
            continue;
        }
        else
        {
            if(line.Contains("@endmindmap")){
                TraceLog("End Mindmap");
                break;
            }
        }

        if(string.IsNullOrEmpty(line)) continue;

        line = line.Replace(',', '、');

        int lineDepth;
        string lineValue;
        {
            Regex regex = new (@"[\s\t*](\*+)[\s\t*](.+)");
            var match = regex.Match(line);        
            if(match.Groups.Count < 3) {
                //この正規表現に該当しない場合は、想定外の内容の行なので、スキップして処理を続ける                
                TraceLog($"Skip Line (Regex Unmatch), Line={line}");
                continue;
            }
            lineDepth = match.Groups[1].Value.Length;
            lineValue = match.Groups[2].Value;
        }

        if(lastDepth == lineDepth)
        {
            //同一の深さの場合は、前の行の最後の要素だけを差し替えて新しい行にする
            lastCsvLine[lastCsvLine.Count - 1] = lineValue;
            outputBuf.Add(string.Join(',', lastCsvLine));   
            lastDepth = lineDepth;
        }
        else if(lastDepth < lineDepth)
        {
            //深さが増えた場合は、その前の行は枝ではないので、要素を1つ追加した新しい行で上書きする。
            //MindMapの仕様上、深さは1つずつしか増えないはずなので、2つ以上増えることは考慮しない
            lastCsvLine.Add(lineValue);
            var newLine = string.Join(',', lastCsvLine);
            if(outputBuf.Any())
            {
                outputBuf[outputBuf.Count - 1] = newLine; 
            }
            else
            {
                outputBuf.Add(newLine);
            }
            
            lastDepth = lineDepth;
        }
        else
        {
            //深さが減った場合、前の行を同一の深さまで減らして最後の要素だけを差し替えたものを、新しい行にする
            lastCsvLine.RemoveRange(lineDepth, lastDepth - lineDepth);
            lastCsvLine[lastCsvLine.Count - 1] = lineValue;
            outputBuf.Add(string.Join(',', lastCsvLine));  
            lastDepth = lineDepth;   
        }
          

    }


}

File.WriteAllLines(outputFilePath, outputBuf);



#endregion

