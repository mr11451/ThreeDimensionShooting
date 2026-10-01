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
│  │  │  ├─ Enemy/          # 敵(基底/追跡型 など)
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
| 右スティック | 視点 / 機体の向き |
| RB | 前進 |
| RT | メインショット(連射) |
| LT ホールド | ロックオン(範囲内の敵を複数捕捉) |
| LT 解除 | ロックオンミサイル発射 |
| LB | 後退 |
| Start | ポーズ(検討中) |

## セットアップ

1. Unity Hub で Unity 6000.0.84f1 をインストール(済)
2. Unity Hub で `Game` フォルダを開く
3. 初回起動時に Input System が有効化され、再起動を求められる場合あり

## 現状(プロトタイプ)

- [x] プロジェクト新規作成(Built-in RP)
- [x] フォルダ構成 + .gitignore
- [x] 自機コントローラー(PlayerShipController)
- [x] 武装の骨組み(PlayerWeapons)
- [x] シールド自動回復(PlayerShield)
- [x] 敵基底 + 追跡型(EnemyBase / ChaserEnemy)
- [x] ゲーム進行(GameManager: スコア・コンボ・ウェーブ)
- [x] ワイヤーフレームシェーダー(WireframeUnlit)
- [x] シーン作成(Title / Game / GameOver)+ EditorBuildSettings 登録
- [x] WaveManager(敵スポーン制御の骨組み)
- [x] タイトル / ゲームオーバー画面の基本コントローラー
- [x] 敵弾・ミサイル実体・ロックオン処理(Bullet / Missile / PlayerWeapons)
- [x] 敵プレハブ(ワイヤーフレーム見た目)作成(ChaserEnemy)
- [x] HUD(シールド/スコア/コンボ/ミサイル/ウェーブ表示)
- [x] 敵の射撃(EnemyBullet / ChaserEnemy の Fire 実装)
- [x] ゲームオーバー → GameOver シーン遷移
- [x] 効果音(プロシージャル生成: ショット/ミサイル/爆発/被弾/ロックオン)
- [x] BGM(プロシージャル生成: シンセ系ループ)
- [ ] ハイスコア保存

## 仕様

詳細は [designe.md](designe.md) を参照。
