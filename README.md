# BRBouyomi
[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)でタグ辞書に登録した単語で改行するプラグイン。  
  
## Description
棒読みちゃん専用プラグイン。  
棒読みちゃん読み上げタスクを受信し、メッセージをReplaceTag.dicに登録された単語で切り分け、棒読みちゃん読み上げタスクに分割登録します。  
  
主に以下の状況改善を想定。  
[Voiceroid Talk Plus](https://ch.nicovideo.jp/Wangdora/blomaga/ar12646)使用時、再生タグ(Sound/SoundW)を発言に含めるとVoiceroidではなく棒読みちゃんでの読み上げになってしまう。  
(exVoiceとVoiceroidを共存させたかった。)  
  
## Demo
読み上げ「こんにちは今日も良い天気ですね。」
- タグ辞書
   - 探索文字列：こんにちは  
   - 置換後：(soundW こんにちは.wav)  
  
#### 通常
こんにちは今日も良い天気ですね。  
###### 読み上げ内容
>(soundW こんにちは.wav)今日も良い天気ですね。  
  
#### 切り分け後
こんにちは  
今日も良い天気ですね。  
###### 読み上げ内容
>(soundW こんにちは.wav)  
>今日も良い天気ですね。  
  
## Usage
棒読みちゃん読み上げタスク待受にTCP通信を使用します。  
**IPAddress：127.0.0.1**  
**Port：50005**  
※現在固定値です。設定で変更できるように改善予定。  
  
上記アドレスに読み上げ指示を送信して下さい。  
送信内容はBouyomiChanのSampleSrcに同梱されている「Socket通信で読み上げ指示を送る」を参考にして下さい。  
  
## Install
「Plugin_BRBouyomi.dll」を「BouyomiChan.exe」と同フォルダに配置。  
  
## Uninstall
「BouyomiChan.exe」と同フォルダに配置された「Plugin_BRBouyomi.dll」を削除。  
  
## Requirement
[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)専用。  
  
## Related
#### [Voiceroid Talk Plus](https://ch.nicovideo.jp/Wangdora/blomaga/ar12646)
Voiceroidとの連携に使用。  
#### [Voiceroid](https://www.ah-soft.com/product/series.html#voiceroid)
Voiceroid本体。あかりちゃんかわいい。  
### Extra
#### [DiSpeak](https://github.com/micelle/dc_DiSpeak)
Discordとの連携に使用。  
  
## Licence
This software is released under the MIT License, see LICENSE.  
  
## Author
[makiryk](https://github.com/makiryk)  