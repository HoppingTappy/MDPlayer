# MDPlayer
VGMファイルなどのPlayer(メガドライブ音源チップなどのエミュレーションによる演奏ツール)  
  
[概要]  
  このツールは、鍵盤表示を行いながらVGMファイルの再生を行います。  
  (NRD,XGM,S98,MID,RCP,NSF,HES,SID,MDR,MDX,MND,MUC,MUBファイルにも対応。)  
  
[注意]  
  ・再生時の音量に注意してください。バグによる雑音が大音量で再生される場合もあります。  
  (特に再生したことのないファイルを試す場合や、プログラムを更新した場合。)  
  
  ・使用中に不具合を見つけた場合はお手数ですが以下までご連絡ください。  
    Twitter(@kumakumakumaT_T)  
    Github Issues(https://github.com/kuma4649/MDPlayer/issues)  
  (VGMPlayやNRTDRV、その他素晴らしいソフトウェアの作者様方に、  
  直接MDPlayerについての連絡がいくことの無い様にお願いします。)  
  できるかぎり対応させていただくつもりですが、ご希望に添えないことも多々あります。ご了承ください。  
  
[対応フォーマット]  
  .VGM (所謂vgmファイル)  
  .VGZ (vgmファイルをgzipしたもの)  
  .NRD (NRTDRV X1でOPM2個とAY8910を鳴らすドライバの演奏ファイル)  
  .XGM (MegaDrive向けファイル)  
  .S98 (主に日本製レトロPC向けファイル)  
  .MID (StandardMIDIファイル。フォーマット0/1対応)  
  .RCP (レコポンファイル CM6,GSDの送信可)  
  .NSF (NES Sound Format)  
  .HES (HESファイル)  
  .SID (コモドール向けファイル)  
  .MDR (MoonDriver MSXで,MoonSound(OPL4)を鳴らすドライバの演奏ファイル)  
  .MDX (MXDRV向けファイル)  
  .MND (MNDRV X68000(OPM,OKIM6258) & まーきゅりーゆにっと(OPNAx2)を使用するドライバの演奏ファイル)  
  .MUC (MUCOM88Windows 向けファイル)  
  .MUB (MUCOM88Windows 向けファイル)  
  .M3U (プレイリスト)  
  
[機能、特徴]  
  ・現在、以下の主にメガドライブ系音源チップのエミュレーションによる再生が可能です。  
     
      AY8910    , YM2612   , SN76489 , RF5C164 , PWM     , C140(C219) , OKIM6295 , OKIM6258(PCM8,MPCM含)  
      , SEGAPCM , YM2151   , YM2203  , YM2413  , YM2608  , YM2610/B   , HuC6280  , C352  
      , K054539 , NES_APU  , NES_DMC , NES_FDS , MMC5    , FME7       , N160     , VRC6  
      , VRC7    , MultiPCM , YMF262  , YMF271  , YMF278B , YMZ280B    , DMG      , QSound  
      , S5B     , GA20  
      , RF5C68  , SID      , Y8950   , YM3526  , YM3812  , K053260    , K051649(K052539)  
  
  ・現在、以下の鍵盤表示が可能です。  
     
      YM2612        , SN76489    , RF5C164  
      , AY8910      , C140(C219) , C352    , SEGAPCM  
      , Y8950       , YM2151     , YM2203  , YM2413  , YM2608 , YM2610/B , YM3526 , YM3812  
      , YMF262      , YMF278B    
      , HuC6280     , MIDI       
      , NES_APU&DMC , NES_FDS    , MMC5    , VRC7    
  
  ・C#で作成されています。  
  
  ・VGMPlay,MAME,DOSBOXのソースを参考、移植しています。  
  
  ・FMGenのソースを参考、移植しています。  
  
  ・NSFPlayのソースを参考、移植しています。  
  
  ・NEZ Plug++のソースを参考、移植しています。  
  
  ・libsidplayfpのソースを参考、移植しています。  
  
  ・sidplayfpのソースを参考、移植しています。  
  
  ・NRTDRVのソースを参考、移植しています。  
  
  ・MoonDriverのソースを参考、移植しています。  
  
  ・MXPのソースを参考、移植しています。  
  
  ・MXDRVのソースを参考、移植しています。  
  
  ・MNDRVのソースを参考、移植しています。  
  
  ・X68Soundのソースを参考、移植しています。  
  (m_puusanさん/rururutanさん版両方)  
  
  ・MUCOM88/MUCOM88windowsのソースを参考、移植しています。  
  
  ・CVS.EXEの出力を参考に同じデータが出力されるよう調整しています。  
  
  ・SCCIを利用して本物のYM2612,SN76489,YM2608,YM2151,YMF262から再生が可能です。  
  またSPPCMにも対応しています。  
  
  ・GIMIC(C86ctl)を利用して本物のYM2608,YM2151,YMF262から再生が可能です。  
  
  ・ボタンは以下の順に並んでいます。  
     
     設定、停止、一時停止、フェードアウト、前の曲、1/4速再生、再生、4倍速再生、次の曲、  
     プレイモード、ファイルを開く、プレイリスト、  
     情報パネル表示、ミキサーパネル表示、パネルリスト表示、VSTeffectの設定、MIDI鍵盤表示、表示倍率変更  
  
  ・チャンネル(鍵盤)を左クリックすることでマスクさせることができます。  
    右クリックすると全チャンネルのマスクを解除します。  
    (いろいろなレベルで対応していないのもあり)  
  
  ・OPN,OPM,OPL系の音色パラメーターを左クリックするとクリップボードに音色パラメーターをテキストとしてコピーします。  
  パラメーターの形式はオプション設定から変更可能です。  
     
      FMP7 , MDX , MUCOM88(MUSIC LALF) , NRTDRV , HuSIC , MML2VGM , .TFI , MGSC  
  
  に対応しており、.TFIを選んだ場合はクリップボードの代わりにファイルに出力します。  
  
  ・出来は今一歩ですが、YM2612 , YM2151 の演奏データをMIDIファイルとして出力が可能です。  
  VOPMexを使用すれば、FM音源の音色情報も反映させることが可能です。  
  (VOPMではなく、VOPMexです。;-P )  
  
  ・PCMデータをダンプすることができます。SEGAPCMの場合のみWAVで出力します。  
  
  ・演奏をwavで書き出すことが可能です。  
  
  ・MIDI音源にVSTiを指定可能です。  
  
  ・キーボード、MIDIキーボードから、再生、停止などの操作が可能です。  
  
  ・プレイリストから、再生中の拡張子違いの同名ファイル(Text,MML,Image)を開くことができます。  
  
  ・VGM/VGZファイルに独自機能を追加しています。  
      RF5C164のDual演奏  
      歌詞表示  
  
  
[G.I.M.I.C.関連情報]  
  ・SSG volumeについて  
    SSG volumeは、ミキサー画面右下の「G.OPN」「G.OPNA」フェーダーで調節してください。  
    それぞれ  
      G.OPN    ->  YM2203(Pri/Sec)に設定したG.I.M.I.C.のモジュール  
      G.OPNA   ->  YM2608(Pri/Sec)に設定したG.I.M.I.C.のモジュール  
    に設定情報が送信されます。  
    なお、設定は再生開始時にのみ送信されます。  
    よって演奏中にフェーダーを動かしてもその値が即時反映されることはありません。  
    初期値としては、  
      .muc(mucom88)  ->  63 (PC-8801-11相当)  
      .mub(mucom88)  ->  63 (PC-8801-11相当)  
      .mnd(MNDRV)    ->  31 (PC-9801-86相当)  
      .s98           ->  31 (PC-9801-86相当)  
      .vgm           ->  31 (PC-9801-86相当)  
    を設定しています。  
    必要に応じて、ドライバ毎又はファイル毎にバランスを調節し、  
    保存(ミキサー画面で右クリックすると保存メニューを表示)してください。  
    
    また、以下の演奏ファイルは、ファイル内に記述されているタグを判別して自動で設定することも可能です(TBD)。
    
    .S98ファイル  
    「system」タグ内に「8801」という文字を見つけるとMDPlayerは「63」を設定します。  
    「9801」という文字を見つけるとMDPlayerは「31」を設定します。  
    両方見つけた場合は「8801」を優先します。  
    見つからない場合は、通常の動作になります。  
    
    .vgmファイル  
    「systemname」「systemnamej」タグ内に「8801」という文字を見つけるとMDPlayerは「63」を設定します。  
    「9801」という文字を見つけるとMDPlayerは「31」を設定します。  
    両方見つけた場合は「8801」を優先します。  
    見つからない場合は、通常の動作になります。  
    
    
  ・周波数について  
    ファイル形式ごとにモジュールの周波数(チップのマスタークロック)の設定を行います。  
    設定値は以下の通りです。  
      .vgm           ->  ファイル中に設定されている値を使用  
      .s98           ->  ファイル中に設定されている値を使用  
      .muc(mucom88)  ->  OPNA:7987200Hz  
      .nrd(NRTDRV)   ->  OPM:4000000Hz  
      .mdx(MXDRV)    ->  OPM:4000000Hz  
      .mnd(MNDRV)    ->  OPM:4000000Hz  OPNA:8000000Hz  
  
  
[必要な動作環境]  
  ・恐らく、WindowsVista(32bit)以降のOS。私はWindows10Home(x64)を使用しています。  
  XPでは動作しません。  
  
  ・.NET Framework4.5/4.5.2をインストールしている必要あり。  
  
  ・Visual Studio 2012 更新プログラム 4 の Visual C++ 再頒布可能パッケージ をインストールしている必要あり。  
  
  ・Microsoft Visual C++ 2015 Redistributable(x86) - 14.0.23026をインストールしている必要あり。  
  
  ・LZHファイルを使用する場合はUNLHA32.DLL(Ver3.0以降)をインストールしている必要あり。  
  
  ・音声を再生できるオーディオデバイスが必須。  
  そこそこ性能があるものが必要です。UMX250のおまけでついてたUCA222でも十分いけます。私はこれを使ってました。  
  
  ・もしあれば、SPFM Light＋YM2612＋YM2608＋YM2151＋SPPCM  
  
  ・もしあれば、GIMIC＋YM2608＋YM2151  
  
  ・YM2608のエミュレーション時、リズム音を鳴らすために以下の音声ファイルが必要です。  
  作成方法は申し訳ありませんがお任せします。  
      
      バスドラム      2608_BD.WAV  
      ハイハット      2608_HH.WAV  
      リムショット    2608_RIM.WAV  
      スネアドラム    2608_SD.WAV  
      タムタム        2608_TOM.WAV  
      トップシンバル  2608_TOP.WAV  
      (44.1KHz 16bitPCM モノラル 無圧縮Microsoft WAVE形式ファイル)  
  
  ・YMF278Bのエミュレーション時、MoonSoundの音色を鳴らすために以下のROMファイルが必要です。  
  作成方法は申し訳ありませんがお任せします。  
  	yrw801.rom  
  
  ・C64のエミュレーション時、以下のROMファイルが必要です。  
  作成方法は申し訳ありませんがお任せします。  
  	Kernal , Basic , Character  
  
  ・そこそこ速いCPU。  
  使用するChipなどによって必要な処理量が変わります。  
  私はi7-9700K 3.6GHzを使用しています。  
  
  
[SpecialThanks]  
  本ツールは以下の方々にお世話になっております。また以下のソフトウェア、ウェブページを参考、使用しています。  
  本当にありがとうございます。  
     
    ・ラエル さん  
    ・とぼけがお さん  
    ・HI-RO さん  
    ・餓死3 さん  
    ・おやぢぴぴ さん  
    ・osoumen さん  
    ・なると さん  
    ・hex125 さん  
    ・Kitao Nakamura さん  
    ・くろま さん  
    ・かきうち さん  
    ・ぼう☆きち さん  
    ・dj.tuBIG/MaliceX さん  
    ・じごふりん さん  
    ・WING さん  
    ・そんそん さん  
    ・欧場豪 さん  
    ・sgq1205 さん  

    ・Visual Studio Community 2015/2017  
    ・MinGW/msys  
    ・gcc  
    ・SGDK  
    ・VGM Player  
    ・Git  
    ・SourceTree  
    ・さくらエディター  
    ・VOPMex  
    ・NRTDRV  
    ・MoonDriver  
    ・MXP  
    ・MXDRV  
    ・MNDRV  
    ・MPCM  
    ・X68Sound  
    ・hoot  
    ・XM6 TypeG  
    ・ASLPLAY  
    ・NAUDIO  
    ・VST.NET  
    ・NSFPlay  
    ・CVS.EXE  
    ・KeyboardHook3.cs  
    ・MUCOM88  
    ・MUCOM88windows  
    ・C86ctlのソース  
     
    ・SMS Power!  
    ・DOBON.NET  
    ・Wikipedia  
    ・GitHub  
    ・ぬるり。  
  
  
[FAQ]  
  
  ・起動しない  
  
    Case1  
      主に実チップ使用時に発生します。SCCIがc86ctlを使用する状態になっている為です。  
    MDPlayerもc86ctlを使用するため取り合いになってしまい、起動に失敗します。  
    →scciconfig.exeを使用してc86ctlの設定項目である「enable」のチェックを外してください。  
  
    CaseX  
      TBD  
  
  
  ・テンポが安定しない、演奏開始時に曲の初めが演奏されない、早送りになる  
  
    Case1  
      主に実チップ使用時に発生します。実チップは演奏開始時の処理に少し時間がかかります。  
    一方エミュレーションの演奏開始時の処理はすぐに完了します。  
    その時間差を詰めるために実チップがエミュレーションに追い付こうとする為です。  
    →「オプション」画面：「音源」タブ：左下の「日和見〜」のチェックボックスからチェックを外してください。  
  
    CaseX  
      TBD  
  
  
[同期のすゝめ]  
    
  ・SCCI/GIMIC(C86ctl)とエミュレーション(以下EMUと略す)による音を同期させるのにはコツがいります。  
  環境にもよるので何が正解かはわからないのですが、私の環境での調整手順を紹介します。  
      
    １．まず、[出力]タブから音声の出力に使用するデバイスを選びます。  
    おすすめはWasapiOutで共有を選ぶ、又はASIOを選ぶパターンです。  
      
    ２．遅延時間は50msか100msを選びます。ここで一度[OK]を押してEMUのみを使用する曲を再生し  
    音がざらざらしたりプチプチといったノイズが混ざらないことを確認します。  
    (もし綺麗に再生されない場合は遅延時間をひとつ大きく設定します。)  
      
    ３．[音源]タブからYM2612のSCCIを選択し使用するモジュールを選択します。  
    SCCIのみ  
    チェックボックスは「Waitシグナル発信」と「PCMだけエミュレーションする」にチェックを入れてください。  
    「Waitシグナル発信」を行うとSCCIのテンポが安定するようです。  
    しかし「そのWait値を2倍」にチェックするとPCMの音質は上がりますがテンポが乱れる傾向があります。  
      
    ４．遅延演奏のグループはとりあえずSCCI/GIMICもEMUも0msを設定し「日和見モード」にはチェックをいれてください。  
    「日和見モード」は、例えば演奏中に大きな負荷がかかり、SCCI/GIMICの再生とEMUの再生が大きくずれた場合に  
    SCCI/GIMICの再生スピードを調整してズレを軽減させる機能です。但し、遅延演奏で設定した(意図した)ズレは保ち続けます。  
      
    ５．SCCI/GIMICとEMUの両方が使用されている曲を再生し、どちらが先に鳴っているか注意深く確認します。  
    SCCI/GIMICとEMUのうち先に演奏されている方の遅延演奏時間を増やし曲再生を行い確認します。  
      
    ６．５の手順をズレがなくなるまで繰り返せば同期作業は完了です。楽しんで！  
      
    ７．SCCIとGIMICの演奏ずれについて。  
    SCCIが早い場合はSCCIのディレイ設定項目を調整します。  
    GIMICが早い場合はGIMICのディレイ設定項目を調整します。  
  
  
[MIDI鍵盤のすゝめ]  
  ・MIDIキーボードを用意すると、それを使用してYM2612(EMU)から発音させることができます。  
  これは主にMML打ち込み支援のために用意された機能です。  
  (今のところ実装途中の状態で使用できない機能があります。)  
  
  ・とりあえずの使い方  
      
    １. 設定画面で、使用するMIDIキーボードを選択します。  
       
    ２. YM2612のデータを再生中に(CC:97)を送信します。  
        YM2612の1Chの音色が全てのチャンネルへセットされます。  
       
    ３. 後は弾くだけ。  
    
  ・主な機能  
      
    １. 音色データ取り込み  
      各OPN系音源又はOPM音源、鍵盤表示の音色データ部をクリックすると  
      音色データが選択チャンネルへコピーされます。  
      
    ２. 演奏モード切替  
      MONOモード(単一チャンネルを使用して演奏)と  
      POLYモード(複数チャンネル(最大6Ch)を使用して演奏)を  
      切り替えることが可能です。  
      MONOモード  打ち込み時に短いフレーズを演奏し、MMLとして出力することを想定。  
      POLYモード  打ち込み時に和音を確認するために使用することを想定。  
      
    ３. チャンネルノートログ  
      チャンネルごとに最大100音の発音記録を残すことができます。  
      
    ４. チャンネルノートログMML変換機能  
      ログ欄をクリックすると発音記録がMMLとしてクリップボードにコピーされます。  
      音長は出力されません。対応コマンドは、c d e f g a b o < > です。  
      オクターブ情報は初めの一音のみoコマンドで絶対指定され、  
      その後は<コマンド>コマンドによる相対指定で展開されます。  
      
    ５. 音色保存、読み込み  
      メモリ上に256種類の音色を保存する、又は読み込むことが可能です。  
      そのデータを、指定された形式でファイルへ出力する、又は読み込むことが可能です。  
      以下のソフトウェア向けの形式で保存、読み込みが可能です。  
        FMP7  
        MUCOM88(MUSIC LALF/mucomMD2vgm)  
        NRTDRV  
        MXDRV  
        mml2vgm  
      
    ６. 簡易音色編集(TBD)  
      入力したいパラメータを選択後、数値を入力することで編集が可能です。  
      
  ・画面  
      
    ０. 鍵盤(TBD)  
      演奏中のノートが表示されます。  
      
    １．MONO  
      クリックするとMONOモードに切り替えます。切り替わると♪アイコンになります。  
      
    ２．POLY  
      クリックするとPOLYモードに切り替えます。切り替わると♪アイコンになります。  
      
    ３．PANIC  
      全チャンネルにキーオフを送信します。(音が鳴り続けてしまう場合に使用します。)  
      
    ４．L.CLS  
      全チャンネルのノートログをクリアします。  
      
    ５．TP.PUT  
      TonePallet(メモリ上の音色保管領域)へ選択チャンネルの音色を保存します。  
      
    ６．TP.GET  
      TonePalletから音色を選択チャンネルへ読み込みます。  
      
    ７．T.SAVE  
      TonePalletをファイルへ保存します。  
      
    ８．T.LOAD  
      ファイルからTonePalletを読み込みます。  
      
    ９．音色データ(6Ch分)  
      ・「-」又は「♪」をクリックすることで チャンネルの選択、選択解除ができます。  
      ・パラメータを右クリックすることで、そのパラメータの変更ができます。(TBD)  
      ・パラメータを左クリックすることで、コンテキストメニューが表示されます。  
        コピー   : クリックした音色をクリップボードにコピーします。  
        貼り付け : クリップボードの音色をクリックした音色にペーストします。  
        上記の機能で使用されるテキスト形式をFORMATの欄のソフトウェア名をクリックすることで変更できます。  
        キー操作でもコピーと貼り付けが可能です。この場合は選択されているチャンネルが対象になります。  
        尚、貼り付け時に形式の自動判別は行われません。  
      ・「LOG」の隣の「♪」をクリックすることで、そのチャンネルのノートログがクリアされます。  
      ・LOGをクリックすることで、MMLデータをクリップボードに設定します。  
      
  ・MIDI鍵盤からの操作  
    以下、デフォルト設定の場合です。(設定でカスタマイズ可能。設定値をブランクにすることで使用しないことも可能。)  
      
    CC:97(DATA DEC)  
      YM2612の1Chの音色を全てのチャンネルにコピーします。(選択状況無視)  
      
    CC:96(DATA INC)  
      直近のログをひとつだけ削除します。(弾き間違い等を取り消す機能)  
      
    CC:66(SOSTENUTO)  
      MONOモード時のみ、選択行のログをMMLにしクリップボードに設定します。  
      画面クリック時の処理との違い  
        Ctrl+V（ペースト）のキーストロークを送信します。  
        選択チャンネルのノートログをクリアします。  
        初めのオクターブコマンドは出力しません。  
      
      
[著作権・免責]  
  MDPlayerはMITライセンスに準ずる物とします。LICENSE.txtを参照。  
  著作権は作者が保有しています。  
  このソフトは無保証であり、このソフトを使用した事による  
  いかなる損害も作者は一切の責任を負いません。  
  また、MITライセンスは著作権表示および本許諾表示を求めますが本ソフトでは不要です。  
  そして以下のソフトウェアのソースコードをC#向けに移植改変、またはそのまま使用しています。  
  これらのソース、ソフトウェアは各著作者が著作権を持ちます。 ライセンスに関しては、各ドキュメントを参照してください。  
  
  ・VGMPlay  
  ・MAME  
  ・DOSBOX  
  ・FMGen  
  ・NSFPlay  
  ・NEZ Plug++  
  ・libsidplayfp  
  ・sidplayfp  
  ・NRTDRV  
  ・MoonDriver  
  ・MXP  
  ・MXDRV  
  ・MNDRV  
  ・X68Sound  
  (m_puusanさん/rururutanさん版両方)  
  ・MUCOM88  
  ・MUCOM88windows  
  ・VST.NET  
  ・NAudio  
  ・SCCI  
  ・c86ctl  
  
  
