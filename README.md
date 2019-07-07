# したらば掲示板のHTMLからDATファイルに変換するやつ

dotnet core 2.2前提。  
現時点ではリプライ指定で使われているspanタグを"&gt;&gt;1"みたいな形にする処理がまだ実装されていません。

- Required
    - [AngleSharp](https://github.com/AngleSharp/AngleSharp)
    - [TaskBuilder.fs](https://github.com/rspeele/TaskBuilder.fs)
    - [CommandLineParser](https://github.com/commandlineparser/commandline)
