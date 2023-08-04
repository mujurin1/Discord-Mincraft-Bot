# Discord-Mincraft-Bot
これは Minecraft のログを Discord に通知するためのアプリです  
（プラグインではありません）

[ここからダウンロード出来ます](https://github.com/mujurin1/Discord-Mincraft-Bot/releases)


## 使い方
Minecraft の .jar や server.properties のあるフォルダに実行ファイルを移動して、起動して下さい  
初回実行時に、Discord の WebhookUrl を入力して下さい  
入力後に noticeApp.yml が生成されます


## noticeApp.yml
起動引数や Discord に通知するメッセージを保存しているファイルです  
このアプリの実行中にこのファイルを保存すると、自動的に変更が反映されます  
(内容が不正な場合は変更を反映せずに、反映できない理由をコンソールに表示します)

Discord Minecraft Bot アプリが更新された場合や、内容が不正な場合に noticeApp.old.yml が生成される場合があります  
この時には noticeApp.yml が新規作成されますが、以前の内容を引き継いでいない場合があるため、 noticeApp.old.yml を見て編集して下さい


## noticeApp.yml の内容
アプリの設定ファイルです。yaml記法で記述します  
`#` 以降の文字はコメント (何を書いてもいい) になるので、メモは `#` を使って下さい

アプリの実行中にこのファイルを変更すると、変更が即時に反映されます

* `DontEditThisValue`
  * この値は編集しないで下さい！
* `Java`
  * java のパスを入力して下さい。普通変更する必要はないです
* `StartupArg`
  * java の起動引数 (run.bat の java 以降の文字列のことです)
* `WebhookUrl`
  * 通知する Discord のサーバーの WebhookUrl
* `BotName`
  * ボットの名前 (空白の場合は設定したウェブフックの名前)
* `Message`
  * ※※実際にはこの行の右側には何も書かないで下さい※※
  * この中の項目が通知メッセージです
  * 空白の場合は通知されません
  * 通常のメッセージと同じように、先頭に `@silent` を付けるとサイレントメッセージになります
  * `OpendServer`
    * サーバーが起動したら通知します
  * `ClosedServer`
    * サーバーが終了したら通知します
  * `Join`
    * 誰かが参加したら通知します
    * `{name}` の部分がプレイヤー名に置き換わります
  * `Left`
    * 誰かが抜けたら通知します
    * `{name}` の部分がプレイヤー名に置き換わります



