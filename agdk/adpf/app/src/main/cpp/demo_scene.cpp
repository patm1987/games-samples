/*
 * Copyright 2022 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include "demo_scene.h"

#include <cassert>
#include <functional>

#include "Log.h"
#include "adpf_manager.h"
#include "imgui.h"
#include "imgui_manager.h"
#include "native_engine.h"

extern "C" {
#include <GLES2/gl2.h>
}

namespace {
const ImVec4 TEXTCOLOR_WHITE = ImVec4(1.0f, 1.0f, 1.0f, 1.0f);
const ImVec4 TEXTCOLOR_GREY = ImVec4(0.7f, 0.7f, 0.7f, 1.0f);
const ImVec4 TEXTCOLOR_RED = ImVec4(1.0f, 0.2f, 0.2f, 1.0f);
const ImVec4 TEXTCOLOR_GREEN = ImVec4(0.2f, 1.0f, 0.2f, 1.0f);
}  // namespace

// String labels that represents thermal states.
const char* thermal_state_label[] = {
    "THERMAL_STATUS_NONE",     "THERMAL_STATUS_LIGHT",
    "THERMAL_STATUS_MODERATE", "THERMAL_STATUS_SEVERE",
    "THERMAL_STATUS_CRITICAL", "THERMAL_STATUS_EMERGENCY",
    "THERMAL_STATUS_SHUTDOWN"};

//--------------------------------------------------------------------------------
// Ctor
//--------------------------------------------------------------------------------
DemoScene::DemoScene() {
  simulated_click_state_ = SIMULATED_CLICK_NONE;
  pointer_down_ = false;
  point_x_ = 0.0f;
  pointer_y_ = 0.0f;
  transition_start_ = 0.0f;
  target_frame_period_ = SWAPPY_SWAP_60FPS;
  current_frame_period_ = SWAPPY_SWAP_60FPS;

  box_.Init();
  InitializePhysics();
}

//--------------------------------------------------------------------------------
// Dtor
//--------------------------------------------------------------------------------
DemoScene::~DemoScene() {
  box_.Unload();
  CleanupPhysics();
}

//--------------------------------------------------------------------------------
// Callbacks that manage demo scene's events.
//--------------------------------------------------------------------------------
void DemoScene::OnStartGraphics() { transition_start_ = Clock(); }

void DemoScene::OnKillGraphics() {}

void DemoScene::OnScreenResized(int width, int height) {}

//--------------------------------------------------------------------------------
// Process each frame's status updates.
// - Initiate the OpenGL rendering.
// - Monitor the device's thermal staus using ADPF API.
// - Update physics using BulletPhysics.
// - Render cubes.
// - Render UI using ImGUI (Show device's thermal status).
// - Tell the system of the samples workload using ADPF API.
//--------------------------------------------------------------------------------
void DemoScene::DoFrame() {
  // Tell ADPF manager beginning of the perf intensive task.
  ADPFManager::getInstance().BeginPerfHintSession();

  // clear screen
  glClearColor(0.0f, 0.0f, 0.25f, 1.0f);
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
  glDisable(GL_DEPTH_TEST);

  // Update ADPF status.
  ADPFManager::getInstance().Monitor();

  // Update target frame  rate based on the ADPF status.
  auto current_thermal_status = ADPFManager::getInstance().GetThermalStatus();

  // Set target frame rate to 30 FPS when the thermal status is 'moderate' or
  // worse.
  auto constexpr kThrottlingThreshold = 2;
  if (current_thermal_status >= kThrottlingThreshold) {
    target_frame_period_ = SWAPPY_SWAP_30FPS;
  } else {
    target_frame_period_ = SWAPPY_SWAP_60FPS;
  }

  // Set FPS in Swappy.
  if (current_frame_period_ != target_frame_period_) {
    if (SwappyGL_isEnabled()) {
      SwappyGL_setSwapIntervalNS(target_frame_period_);
    }
    current_frame_period_ = target_frame_period_;
  }

  UpdatePhysics();

  // Update UI inputs to ImGui before beginning a new frame
  UpdateUIInput();
  ImGuiManager* imguiManager = NativeEngine::GetInstance()->GetImGuiManager();
  imguiManager->BeginImGuiFrame();
  RenderUI();
  imguiManager->EndImGuiFrame();

  glEnable(GL_DEPTH_TEST);

  // Tell ADPF manager end of the perf intensive task.
  // The ADPF manager update PerfHintManager's session using
  // reportActualWorkDuration() and updateTargetWorkDuration() API.
  ADPFManager::getInstance().EndPerfHintSession();
}

//--------------------------------------------------------------------------------
// Render Background.
//--------------------------------------------------------------------------------
void DemoScene::RenderBackground() {
  // base classes override this to draw background
}

//--------------------------------------------------------------------------------
// Pointer and touch related implementations.
//--------------------------------------------------------------------------------
void DemoScene::OnPointerDown(int pointerId,
                              const struct PointerCoords* coords) {
  // If this event was generated by something that's not associated to the
  // screen, (like a trackpad), ignore it, because our UI is not driven that
  // way.
  if (coords->is_screen_) {
    pointer_down_ = true;
    point_x_ = coords->x_;
    pointer_y_ = coords->y_;
  }
}

void DemoScene::OnPointerMove(int pointerId,
                              const struct PointerCoords* coords) {
  if (coords->is_screen_ && pointer_down_) {
    point_x_ = coords->x_;
    pointer_y_ = coords->y_;
  }
}

void DemoScene::OnPointerUp(int pointerId, const struct PointerCoords* coords) {
  if (coords->is_screen_) {
    point_x_ = coords->x_;
    pointer_y_ = coords->y_;
    pointer_down_ = false;
    simulated_click_state_ = SIMULATED_CLICK_NONE;
  }
}

void DemoScene::UpdateUIInput() {
  ImGuiIO& io = ImGui::GetIO();
  io.MousePos = ImVec2(point_x_, pointer_y_);
  bool pointer_down = false;
  // To make a touch work like a mouse click we need to sequence the following:
  // 1) Position cursor at touch spot with mouse button still up
  // 2) Signal mouse button down for a frame
  // 3) Release mouse button (even if touch is still held down)
  // 4) Reset to allow another 'click' once the touch is released
  if (simulated_click_state_ == SIMULATED_CLICK_NONE && pointer_down_) {
    simulated_click_state_ = SIMULATED_CLICK_DOWN;
  } else if (simulated_click_state_ == SIMULATED_CLICK_DOWN) {
    pointer_down = true;
    simulated_click_state_ = SIMULATED_CLICK_UP;
  }
  io.MouseDown[0] = pointer_down;
}

//--------------------------------------------------------------------------------
// ImGUI related UI rendering.
//--------------------------------------------------------------------------------
void DemoScene::RenderUI() {
  SetupUIWindow();

  ImGui::End();
  ImGui::PopStyleVar();
}

void DemoScene::SetupUIWindow() {
  ImGuiIO& io = ImGui::GetIO();
  const float windowStartY = NativeEngine::GetInstance()->GetSystemBarOffset();
  ImVec2 windowPosition(0.0f, windowStartY);
  ImVec2 minWindowSize(io.DisplaySize.x * 0.95f, io.DisplaySize.y);
  ImVec2 maxWindowSize = io.DisplaySize;
  ImGui::SetNextWindowPos(windowPosition);
  ImGui::SetNextWindowSizeConstraints(minWindowSize, maxWindowSize, NULL, NULL);
  ImGuiWindowFlags windowFlags =
      ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoCollapse |
      ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoBackground;

  ImGui::PushStyleVar(ImGuiStyleVar_ScrollbarSize, 32.0f);
  char titleString[64];
  snprintf(titleString, 64, "ADPF Sample");
  ImGui::Begin(titleString, NULL, windowFlags);

  RenderPanel();
}

void DemoScene::RenderPanel() {
  int32_t thermal_state = ADPFManager::getInstance().GetThermalStatus();
  assert(thermal_state >= 0 &&
         thermal_state <
             sizeof(thermal_state_label) / sizeof(thermal_state_label[0]));

  // Show current FPS target.
  ImGui::Text("FPS target:%s",
              target_frame_period_ == SWAPPY_SWAP_60FPS ? "60" : "30");

  // Show current thermal state on screen.
  ImGui::Text("Thermal State:%s", thermal_state_label[thermal_state]);
  ImGui::Text("Thermal Headroom:%f",
              ADPFManager::getInstance().GetThermalHeadroom());
}

void DemoScene::OnButtonClicked(int buttonId) {
  // base classes override this to react to button clicks
}

//--------------------------------------------------------------------------------
// Initialize BulletPhysics engine.
//--------------------------------------------------------------------------------
void DemoScene::InitializePhysics() {
  // Initialize physics world.
  collision_configuration_ = new btDefaultCollisionConfiguration();
  dispatcher_ = new btCollisionDispatcher(collision_configuration_);
  overlapping_pair_cache_ = new btDbvtBroadphase();
  solver_ = new btSequentialImpulseConstraintSolver;
  dynamics_world_ = new btDiscreteDynamicsWorld(
      dispatcher_, overlapping_pair_cache_, solver_, collision_configuration_);
  dynamics_world_->setGravity(btVector3(0, -10, 0));

  /// create a few basic rigid bodies

  // the ground is a cube of side 100 at position y = -56.
  // the sphere will hit it at y = -6, with center at -5
  btCollisionShape* ground_shape =
      new btBoxShape(btVector3(btScalar(50.), btScalar(50.), btScalar(50.)));

  collision_shapes_.push_back(ground_shape);

  btTransform groundTransform;
  groundTransform.setIdentity();
  groundTransform.setOrigin(btVector3(0, -56, 0));

  btScalar mass(0.);
  btVector3 local_inertia(0, 0, 0);

  // using motionstate is optional, it provides interpolation capabilities, and
  // only synchronizes 'active' objects
  btDefaultMotionState* myMotionState =
      new btDefaultMotionState(groundTransform);
  btRigidBody::btRigidBodyConstructionInfo rbInfo(mass, myMotionState,
                                                  ground_shape, local_inertia);
  btRigidBody* body = new btRigidBody(rbInfo);

  // add the body to the dynamics world
  dynamics_world_->addRigidBody(body);

  // create a dynamic rigidbody
  btCollisionShape* colShape =
      new btBoxShape(btVector3(kBoxSize, kBoxSize, kBoxSize));
  collision_shapes_.push_back(colShape);

  /// Create Dynamic Objects
  for (auto k = 0; k < kArraySizeY; k++) {
    for (auto i = 0; i < kArraySizeX; i++) {
      for (auto j = 0; j < kArraySizeZ; j++) {
        // rigidbody is dynamic if and only if mass is non zero, otherwise
        // static
        btScalar mass_dynamic(1.f);
        colShape->calculateLocalInertia(mass_dynamic, local_inertia);

        btTransform start_transform;
        start_transform.setIdentity();
        start_transform.setOrigin(btVector3(
            btScalar((-kBoxSize * kArraySizeX / 2) + kBoxSize * 2.0 * i),
            btScalar(10 + kBoxSize * k),
            btScalar((-kBoxSize * kArraySizeZ / 2) + kBoxSize * 2.0 * j)));
        float angle = random();
        btQuaternion qt(btVector3(1, 1, 0), angle);
        start_transform.setRotation(qt);

        // using motionstate is recommended, it provides interpolation
        // capabilities, and only synchronizes 'active' objects
        btDefaultMotionState* motionState_dynamic =
            new btDefaultMotionState(start_transform);
        btRigidBody::btRigidBodyConstructionInfo rbInfo_dynamic(
            mass_dynamic, motionState_dynamic, colShape, local_inertia);
        btRigidBody* body_dynamic = new btRigidBody(rbInfo_dynamic);
        dynamics_world_->addRigidBody(body_dynamic);
      }
    }
  }
}

//--------------------------------------------------------------------------------
// Update phycis world and render boxes.
//--------------------------------------------------------------------------------
void DemoScene::UpdatePhysics() {
  // In the sample, it's looping physics update here.
  // It's intended to add more CPU load to the system to achieve thermal
  // throttling status easily.
  // In the future version of the sample, this part will be update to
  // dynamically adjust the load.
  auto max_steps = 8;
  for (auto steps = 0; steps < max_steps; ++steps) {
    dynamics_world_->stepSimulation(1.f / (60.f * max_steps), 10);
  }

  // Update box renderer.
  box_.BeginMultipleRender();

  // print positions of all objects
  auto box_color = 1;
  for (auto j = dynamics_world_->getNumCollisionObjects() - 1; j >= 0; j--) {
    btCollisionObject* obj = dynamics_world_->getCollisionObjectArray()[j];
    btRigidBody* body = btRigidBody::upcast(obj);
    btTransform trans;
    if (body && body->getMotionState()) {
      body->getMotionState()->getWorldTransform(trans);
    } else {
      trans = obj->getWorldTransform();
    }
    auto shape = body->getCollisionShape();
    auto name = shape->getName();
    if (!strcmp(name, "Box")) {
      auto p = reinterpret_cast<btBoxShape*>(shape);
      auto size = p->getHalfExtentsWithoutMargin();

      // Render boxes
      btScalar m[16];
      trans.getOpenGLMatrix(m);

      // Change the box color (expecting BulletPhysics's object iteration order
      // doesn't change).
      auto c = (box_color % 7 + 1);
      float color[3] = {((c & 0x1) != 0) * 1.f, ((c & 0x2) != 0) * 1.f,
                        ((c & 0x4) != 0) * 1.f};
      box_.RenderMultiple(m, size.getX() * 2, size.getY() * 2, size.getZ() * 2,
                          color);
      box_color++;
    }
  }

  box_.EndMultipleRender();

  // Reset a physics each 10 sec.
  static auto count = 0;
  count++;
  if (count > 600) {
    ResetPhysics();
    count = 0;
  }
}

//--------------------------------------------------------------------------------
// Helper to reset the physics world.
//--------------------------------------------------------------------------------
void DemoScene::ResetPhysics() {
  // Reset physics state.
  auto k = 0;
  auto i = 0;
  auto j = 0;
  for (auto index = dynamics_world_->getNumCollisionObjects() - 1; index >= 0;
       index--) {
    btCollisionObject* obj = dynamics_world_->getCollisionObjectArray()[index];
    btRigidBody* body = btRigidBody::upcast(obj);
    if (body) {
      auto shape = body->getCollisionShape();
      auto name = shape->getName();
      if (!strcmp(name, "Box")) {
        auto p = reinterpret_cast<btBoxShape*>(shape);
        auto size = p->getHalfExtentsWithoutMargin();
        auto stage_size_threshold = 30.f;
        if (size.getX() < stage_size_threshold) {
          // Reset position.
          btTransform transform;
          transform.setIdentity();
          transform.setOrigin(btVector3(
              btScalar((-kBoxSize * kBoxSize / 2) + kBoxSize * 2.0 * i),
              btScalar(10 + kBoxSize * k),
              btScalar((-kBoxSize * kArraySizeZ / 2) + kBoxSize * 2.0 * j)));
          float angle = random();
          btQuaternion qt(btVector3(1, 1, 0), angle);
          transform.setRotation(qt);

          body->setWorldTransform(transform);
          body->setActivationState(DISABLE_DEACTIVATION);

          // Update cube's index.
          j = (j + 1) % kArraySizeZ;
          if (!j) {
            i = (i + 1) % kArraySizeX;
            if (!i) {
              k = (k + 1) % kArraySizeY;
            }
          }
        }
      }
    }
  }
  dynamics_world_->setForceUpdateAllAabbs(true);
}

//--------------------------------------------------------------------------------
// Clean up bullet physics data.
//--------------------------------------------------------------------------------
void DemoScene::CleanupPhysics() {
  // remove the rigidbodies from the dynamics world and delete them
  for (auto i = dynamics_world_->getNumCollisionObjects() - 1; i >= 0; i--) {
    btCollisionObject* obj = dynamics_world_->getCollisionObjectArray()[i];
    btRigidBody* body = btRigidBody::upcast(obj);
    if (body && body->getMotionState()) {
      delete body->getMotionState();
    }
    dynamics_world_->removeCollisionObject(obj);
    delete obj;
  }

  // delete collision shapes
  for (auto j = 0; j < collision_shapes_.size(); j++) {
    btCollisionShape* shape = collision_shapes_[j];
    collision_shapes_[j] = 0;
    delete shape;
  }

  // delete dynamics world
  delete dynamics_world_;

  // delete solver
  delete solver_;

  // delete broadphase
  delete overlapping_pair_cache_;

  // delete dispatcher
  delete dispatcher_;

  delete collision_configuration_;

  // next line is optional: it will be cleared by the destructor when the array
  // goes out of scope
  collision_shapes_.clear();
}
