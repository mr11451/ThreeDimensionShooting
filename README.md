# ThreeDimension

一人称視点で 3D 空間を飛び回り、敵宇宙船を撃墜するスペースシューティングゲーム。

## 概要

- **ジャンル**: 一人称スペースドッグファイト(アーケード風ウェーブ制)
- **エンジン**: Unity 6000.0.84f1 (LTS) / Built-in Render Pipeline
- **入力**: ゲームパッド前提(Xbox コントローラー想定)、新 Input System
- **対象**: Windows PC / 60fps
- **ビジュアル**: レトロワイヤーフレーム + 広大な宇宙空間

## プロジェクト構成

```
ThreeDimension/
├─ Game/                    # Unity プロジェクト
│  ├─ Assets/
│  │  ├─ Scenes/            # Title / Game / GameOver
│  │  ├─ Scripts/
│  │  │  ├─ Game/           # GameManager(進行・スコア・ウェーブ)
│  │  │  ├─ Player/         # 自機(移動/武装/シールド)
│  │  │  ├─ Enemy/          # 敵(追跡型/大型ボス など)
│  │  │  └─ UI/             # HUD
│  │  ├─ Shaders/           # ワイヤーフレームシェーダー
│  │  ├─ Materials/
│  │  ├─ Prefabs/
│  │  └─ Audio/             # BGM / SE
│  ├─ Packages/             # manifest.json (Input System 等)
│  └─ ProjectSettings/
├─ designe.md               # 仕様書
└─ README.md
```

## 操作(ゲームパッド)

| 入力 | 動作 |
| --- | --- |
| 左スティック | 機体の上下左右スラスト |
| 右スティック | 視点 / 機体の向き(倒し量でアナログに旋回速度が変化) |
| RT | 前進(押し込み量でアナログに加速、最大 ForwardThrust) |
| LT | 後退(押し込み量でアナログに加速) |
| RB | メインショット(連射) |
| LB ホールド | ロックオン(範囲内の敵を複数捕捉) |
| LB 解除 | ロックオンミサイル発射 |
| Start | ポーズ(検討中) |

5ウェーブごとに大型ボスが単独で出現し、撃破すると次のウェーブへ進む。

### 調整用設定ファイル

慣性・加速・旋回・ボス誘導弾などは `Game/Assets/StreamingAssets/FlightSettings.txt` (`キー = 値`、`#` でコメント)で調整できる。ビルド版でも `ThreeDimensionShooter_Data/StreamingAssets/` 内を編集すれば再ビルド不要。ファイルの値はシーン保存値より優先される。

| キー | 内容 |
| --- | --- |
| LateralThrust / ForwardThrust | 横・前後の最大加速度 |
| MaxSpeed | 最高速度 |
| TriggerDeadZone | トリガーの遊び |
| CoastDeceleration | 入力を離したときの減速(慣性。0 で止まらない) |
| MaxTurnRate | 右スティック最大時の旋回速度(度/秒) |
| LookDeadZone / LookResponseCurve | 右スティックの遊び / 応答カーブ指数 |
| RollLerp / BankOnStrafeDeg | バンク追従速度 / 横移動時の最大バンク角 |
| BossHp | ボスの耐久度 |
| PlayerShotDamage | メインショット1発のダメージ |
| LockOnMissileDamage | ロックオンミサイル1発のダメージ |
| BossHomingMissileInterval | ボス誘導弾の発射間隔(秒) |
| BossHomingMissilesPerLaunch | ボス誘導弾の一回あたりの発射数 |

## セットアップ

1. Unity Hub で Unity 6000.0.84f1 をインストール(済)
2. Unity Hub で `Game` フォルダを開く
3. 初回起動時に Input System が有効化され、再起動を求められる場合あり

## ビルド方法 (Windows)

### Unity Editor からビルド

1. Unity Hub で `Game` フォルダを開く
2. メニューから **Tools > ThreeDimension > Build Windows** を選択
3. ビルド完了後、`Game/Builds/Windows/ThreeDimensionShooter.exe` を実行

ビルドには `Game/ProjectSettings/EditorBuildSettings.asset` で有効になっている Title / Game / GameOver シーンが使われる。ビルドしてすぐ起動する場合は **Tools > ThreeDimension > Build And Run Windows** を選ぶ。

### コマンドラインからビルド

Unity Editor の実行ファイルとリポジトリのパスを環境に合わせて指定し、コマンドプロンプトまたは PowerShell で実行する。

```powershell
& "<Unity Editorの実行ファイルのフルパス>" -batchmode -quit -projectPath "<リポジトリのパス>\Game" -executeMethod ThreeDimensionShooter.EditorTools.BuildPlayer.BuildWindows
```

例: Unity Hub の標準インストール先を使う場合は、Unity Editor の実行ファイルを `C:\Program Files\Unity\Hub\Editor\6000.0.84f1\Editor\Unity.exe` に置き換える。ビルド成果物は `Game/Builds/Windows/` に出力される。

## 仕様

詳細は [designe.md](designe.md) を参照。
