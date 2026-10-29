# BoxingAI 🥊

An intelligent 3D boxing simulation built in **Unity 6 (6000.6.7f1)** powered by **Unity ML-Agents** reinforcement learning (PPO) and self-play.

---

## 🎮 How to Play / Run the Project

### Prerequisites
- **Unity Editor**: `6000.6.7f1` (Unity 6 LTS)
- **Unity ML-Agents**: Package `com.unity.ml-agents` (v4.1.0)
- **Python (Optional for Training)**: Python 3.10+ with `mlagents` package installed (`pip install mlagents`)

### Quick Start (Unity Editor)
1. Open **Unity Hub** and click **Add** > **Add project from disk**.
2. Select the repository root folder (`BoxingAI`).
3. Open the project using Unity Editor **6000.6.7f1**.
4. In the Project browser, open the scene:
   ```
   Assets/OutdoorsScene.unity
   ```
5. Press the **Play (▶️)** button in the Unity Editor toolbar.

---

## 🕹️ Controls (Heuristic Mode)

You can manually control the boxer to spar against the AI or test combat mechanics. Ensure the **Behavior Parameters** component on the Boxer GameObject has **Behavior Type** set to **Heuristic Only** (or Default when no trained model is attached).

| Action | Control (Keyboard) | Description |
| :--- | :--- | :--- |
| **Move / Strafe** | `W` / `S` / `A` / `D` or `Arrow Keys` | Move forward, backward, strafe left, and strafe right |
| **Turn / Rotate** | `Q` (Turn Left) / `E` (Turn Right) | Rotate the boxer towards or away from the opponent |
| **Lead Jab** | `J` | Quick lead punch (fast recovery, moderate damage) |
| **Jab-Cross Combo** | `K` | Two-punch combo (higher damage, longer cooldown) |
| **Guard / Block** | `Space` or `B` (Hold) | Defensive stance (reduces incoming damage by 75%) |

---

## 🧠 Combat Mechanics & AI Observations

- **Health System**: Each fighter has 100 HP. Connecting punches reduces opponent HP, and landing clean strikes earns reinforcement learning rewards.
- **Defensive Guard**: Blocking an attack mitigates 75% of incoming damage and rewards the defender for successful timing while penalizing the attacker for punching into guard.
- **Hit Reaction**: Clean, unblocked punches trigger the `TakingPunch` reaction animation on the receiving boxer.
- **Knockout**: Reducing opponent HP to 0 ends the round/episode with a decisive win reward.

---

## 🏋️ Training with ML-Agents (Self-Play & PPO)

The project includes an optimized PPO configuration with self-play enabled in [`config/BoxerConfig.yaml`](config/BoxerConfig.yaml).

### 1. Start ML-Agents Training
From your terminal in the repository root:
```bash
# Activate your Python virtual environment where mlagents is installed
source .venv/bin/activate

# Launch training session
mlagents-learn config/BoxerConfig.yaml --run-id=Boxer_SelfPlay_01
```

### 2. Connect the Unity Environment
When the console displays:
```
[INFO] Listening on port 5004. Start training by pressing the Play button in the Unity Editor.
```
Press **Play (▶️)** in the Unity Editor.

### 3. Monitor with TensorBoard
In a separate terminal tab:
```bash
tensorboard --logdir results
```
Navigate to `http://localhost:6006` to watch:
- Cumulative Reward
- Policy Loss & Value Loss
- Self-Play ELO rating progress

### 4. Apply Trained Brain (.onnx)
Trained neural networks are output to `results/Boxer_SelfPlay_01/BoxerBrain.onnx`.
Drag and drop the `.onnx` model into the **Model** slot on the **Behavior Parameters** component of the Boxer Agent GameObject.

---

## 📁 Project Structure

```
BoxingAI/
├── Assets/
│   ├── BoxerAgent.cs               # ML-Agents agent logic, observations, actions, and combat
│   ├── BoxerBrain-*.onnx           # Trained neural network models
│   ├── Characters/Boxer_AJ/        # 3D Boxer character model, textures, and animations
│   │   └── Animations/             # Boxing stance, Walk, Lead Jab, Jab Cross, Taking Punch
│   ├── OutdoorsScene.unity         # Primary boxing ring environment
│   └── Settings/                   # High-Definition Render Pipeline (HDRP) settings
├── config/
│   └── BoxerConfig.yaml            # PPO & Self-Play hyperparameters for mlagents-learn
├── Packages/
│   └── manifest.json               # Package dependencies (ML-Agents, IAP v5, Input System)
└── docs/
    └── HISTORY_CLEANUP_PLAN.md     # Git history hygiene and repo maintenance log
```
