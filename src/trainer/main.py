import sys
import argparse
from typing import Final
from pathlib import Path

import keras
import numpy as np
import tensorflow as tf
from sklearn.model_selection import train_test_split

# Clear import cache of protobuf
for import_path in list(sys.modules):
    if import_path.startswith("google"):
        del sys.modules[import_path]

# Append path to message compatible protobuf
ANOTHER_PROTOBUF_DIR: Final[Path] = Path(__file__).parent / "another-protobuf"
sys.path.append(str(ANOTHER_PROTOBUF_DIR))
from message.pose_message_pb2 import PoseSample

type INPUT = list[tuple[float, float, float]]
type TARGET = int


def main():
    parser = argparse.ArgumentParser(description="Train a Keras model from pose data.")
    parser.add_argument("output", type=Path, help="Model data file path")
    parser.add_argument("input", type=Path, nargs="+", help="Pose data file paths")
    args = parser.parse_args()

    output_path: Path = args.output
    input_paths: list[Path] = args.input

    if output_path.exists():
        print(f"Output file {output_path} already exists.")
        overwrite = input("Overwrite? (y/N): ")
        if overwrite != "y":
            exit(0)

    samples = [load_pose(path) for path in input_paths]
    dataset = list(dataset_generator(samples))
    keras_model = train(dataset, len(samples))
    tflite_model = convert(keras_model)

    with open(output_path, "wb") as f:
        f.write(tflite_model)


def load_pose(path: Path) -> PoseSample:
    pose_sample = PoseSample()
    with open(path, "rb") as f:
        pose_sample.ParseFromString(f.read())

    return pose_sample


def dataset_generator(samples: list[PoseSample]):
    for i, sample in enumerate(samples):
        for frame in sample.frames:
            input: INPUT = [(vec.x, vec.y, vec.z) for vec in frame.joint_vectors]
            target: TARGET = i
            yield input, target


def train(dataset: list[tuple[INPUT, TARGET]], num_classes: int):
    keras_model = keras.Sequential(
        [
            keras.layers.InputLayer((32, 3), name="input"),
            keras.layers.Flatten(name="Flatten"),
            keras.layers.Dense(64, activation="elu", name="Dense"),
            keras.layers.Dropout(0.2, name="Dropout"),
            keras.layers.Dense(num_classes, activation="softmax", name="Output"),
        ]
    )

    keras_model.compile(
        optimizer="adam", loss="sparse_categorical_crossentropy", metrics=["accuracy"]
    )

    x = np.array([i for i, _ in dataset])
    y = np.array([t for _, t in dataset])

    x_train, x_val, y_train, y_val = train_test_split(x, y)

    keras_model.fit(
        x=x_train,
        y=y_train,
        validation_data=(x_val, y_val),
        epochs=10,
        verbose="0",
        callbacks=[ProgressLogger()],
    )

    return keras_model


def convert(keras_model: keras.Model) -> bytes:
    converter = tf.lite.TFLiteConverter.from_keras_model(keras_model)
    tflite_model = converter.convert()
    assert isinstance(tflite_model, bytes)
    return tflite_model


class ProgressLogger(keras.callbacks.Callback):
    @property
    def _epochs(self) -> int:
        assert isinstance(self.params, dict)
        return self.params["epochs"]

    @property
    def _steps(self) -> int:
        assert isinstance(self.params, dict)
        return self.params["steps"]

    def on_train_begin(self, logs=None):
        self._total_steps = self._epochs * self._steps
        self._update_progress(0)

    def on_train_end(self, logs=None):
        self._total_steps = None
        self._update_progress(1)

    def on_epoch_begin(self, epoch, logs=None):
        self._current_epoch = epoch

    def on_epoch_end(self, epoch, logs=None):
        self._current_epoch = None

    def on_train_batch_begin(self, batch, logs=None):
        assert isinstance(self._current_epoch, int)
        current_step = self._current_epoch * self._steps + batch
        progress = current_step / self._total_steps
        self._update_progress(progress)

    def _update_progress(self, progress: float):
        print(f"Progress: {progress:.2f}", end="\r")


if __name__ == "__main__":
    main()
