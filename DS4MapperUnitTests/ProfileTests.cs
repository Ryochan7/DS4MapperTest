using Newtonsoft.Json;
using DS4MapperTest;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;

namespace DS4MapperUnitTests
{
    [TestClass]
    public class ProfileTests
    {
        private string daggerFallProfileStr;
        private Dictionary<string, InputBindingMeta> bindingDict =
            new Dictionary<string, InputBindingMeta>();

        protected Dictionary<string, StickDefinition> knownStickDefinitions =
            new Dictionary<string, StickDefinition>();
        protected Dictionary<string, TriggerDefinition> knownTriggerDefinitions =
            new Dictionary<string, TriggerDefinition>();
        protected Dictionary<string, TouchpadDefinition> knownTouchpadDefinitions =
            new Dictionary<string, TouchpadDefinition>();
        protected Dictionary<string, GyroSensDefinition> knownGyroSensDefinitions =
            new Dictionary<string, GyroSensDefinition>();

        public ProfileTests()
        {
            #region profileJSON
            daggerFallProfileStr = @"{
  ""Name"": ""Daggerfall"",
  ""Description"": ""Daggerfall"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2023-10-27T02:05:40.4042828Z"",
  ""OutputGamepadSettings"": {
    ""Enabled"": false
  },
  ""LightbarSettings"": {
    ""Mode"": ""SolidColor"",
    ""SolidColor"": {
      ""red"": 0,
      ""green"": 0,
      ""blue"": 255
    },
    ""RainbowSecondsCycle"": 5
  },
  ""ActionSets"": [
    {
      ""Index"": 0,
      ""Name"": ""Main"",
      ""ActionLayers"": [
        {
          ""Index"": 0,
          ""Name"": ""Default"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""W""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""S""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""A""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""D""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""DeadZone"": 8,
                ""TrackballFriction"": 8,
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0,
                ""SmoothingEnabled"": true,
                ""SmoothingMinCutoff"": 2.0,
                ""SmoothingBeta"": 0.7
              }
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Space""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""M""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""Escape""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""HoldActionLayer"",
                      ""Layer"": 2
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""RightButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 7,
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""MouseButton"",
                      ""Code"": ""LeftButton""
                    }
                  ]
                }
              ],
              ""Settings"": {
                ""DeadZone"": 0.11764705882352941
              }
            },
            {
              ""Id"": 8,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""LeftShift""
                    }
                  ],
                  ""Settings"": {
                    ""Toggle"": true
                  }
                }
              ]
            },
            {
              ""Id"": 9,
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F1""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F3""
                        }
                      ]
                    }
                  ]
                },
                ""Left"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F4""
                        }
                      ]
                    }
                  ]
                },
                ""Right"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Keyboard"",
                          ""Code"": ""F2""
                        }
                      ]
                    }
                  ]
                }
              },
              ""Settings"": {
                ""DeadZone"": 0.5,
                ""PadMode"": ""FourWayCardinal"",
                ""DeadZoneType"": ""Radial""
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""ApplyActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 12,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""E""
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""Index"": 1,
          ""Name"": ""Mouse Wheel"",
          ""MappedActions"": [
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchCircularAction"",
              ""Clockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelDown""
                      }
                    ]
                  }
                ]
              },
              ""CounterClockwise"": {
                ""Functions"": [
                  {
                    ""Type"": ""NormalPress"",
                    ""OutputActions"": [
                      {
                        ""Type"": ""MouseWheel"",
                        ""Code"": ""WheelUp""
                      }
                    ]
                  }
                ]
              },
              ""Settings"": {
                ""Sensitivity"": 1.0
              }
            },
            {
              ""Id"": 10,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 1,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""Index"": 2,
          ""Name"": ""Meta"",
          ""MappedActions"": [
            {
              ""Id"": 3,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F9""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 4,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""HoldPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F11""
                    }
                  ],
                  ""Settings"": {
                    ""HoldTime"": 100
                  }
                }
              ]
            },
            {
              ""Id"": 2,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F6""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 5,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""RemoveActionLayer"",
                      ""Layer"": 2,
                      ""Settings"": {
                        ""ChangeCondition"": ""Released""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 0,
              ""ActionMode"": ""TouchMouseAction"",
              ""Settings"": {
                ""Sensitivity"": 1.0,
                ""VerticalScale"": 1.0
              }
            },
            {
              ""Id"": 1,
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"": {
                ""Up"": {
                  ""Functions"": [
                    {
                      ""Type"": ""NormalPress"",
                      ""OutputActions"": [
                        {
                          ""Type"": ""Empty""
                        }
                      ]
                    }
                  ]
                },
                ""Down"": {},
                ""Left"": {},
                ""Right"": {}
              },
              ""Settings"": {
                ""DeadZone"": 0.05,
                ""OuterRingRange"": ""OnlyActive""
              }
            },
            {
              ""Id"": 11,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Empty""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": 6,
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""Keyboard"",
                      ""Code"": ""F""
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ],
  ""Mappings"": [
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 0,
      ""InputMappings"": [
        {
          ""Input"": ""LeftTouchpad"",
          ""Action"": 0
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""LT"",
          ""Action"": 6
        },
        {
          ""Input"": ""RT"",
          ""Action"": 7
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": 8
        },
        {
          ""Input"": ""Stick"",
          ""Action"": 9
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        },
        {
          ""Input"": ""X"",
          ""Action"": 12
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 1,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": 10
        }
      ]
    },
    {
      ""ActionSet"": 0,
      ""ActionLayer"": 2,
      ""InputMappings"": [
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": 1
        },
        {
          ""Input"": ""A"",
          ""Action"": 2
        },
        {
          ""Input"": ""Back"",
          ""Action"": 3
        },
        {
          ""Input"": ""Start"",
          ""Action"": 4
        },
        {
          ""Input"": ""Steam"",
          ""Action"": 5
        },
        {
          ""Input"": ""Y"",
          ""Action"": 6
        },
        {
          ""Input"": ""B"",
          ""Action"": 11
        }
      ]
    }
  ]
}";
            #endregion

            List<InputBindingMeta> bindingList = new List<InputBindingMeta>()
            {
                new InputBindingMeta("A", "A", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("B", "B", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("X", "X", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Y", "Y", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Back", "Back", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Start", "Start", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LShoulder", "Left Shoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RShoulder", "Right Shoulder", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LSClick", "Stick Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LeftGrip", "Left Grip", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightGrip", "Right Grip", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("LT", "Left Trigger", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("RT", "Right Trigger", InputBindingMeta.InputControlType.Trigger),
                new InputBindingMeta("Steam", "Steam", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Stick", "Stick", InputBindingMeta.InputControlType.Stick),
                new InputBindingMeta("LeftTouchpad", "Left Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("RightTouchpad", "Right Touchpad", InputBindingMeta.InputControlType.Touchpad),
                new InputBindingMeta("LeftPadClick", "Left Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("RightPadClick", "Right Pad Click", InputBindingMeta.InputControlType.Button),
                new InputBindingMeta("Gyro", "Gyro", InputBindingMeta.InputControlType.Gyro),
            };

            // Populate Input Binding dictionary
            bindingList.ForEach((item) => bindingDict.Add(item.id, item));
        }

        [TestMethod]
        public void ReadProfileData()
        {
            Profile tempProfile = new Profile();
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = daggerFallProfileStr;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);
        }

        [TestMethod]
        public void WriteProfileJSON()
        {
            Profile tempProfile = new Profile();
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = daggerFallProfileStr;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);


            string tempOutJson = JsonConvert.SerializeObject(profileSerializer, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    //Converters = new List<JsonConverter>()
                    //{
                    //    new MapActionSubTypeConverter(),
                    //}
                    //TypeNameHandling = TypeNameHandling.Objects
                    //ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            Assert.AreEqual(daggerFallProfileStr, tempOutJson);
        }

        private void FillMappingProfileInitialData(Profile tempProfile)
        {
            // Populate ActionLayer dicts with default no action elements
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                int layerIndex = 0;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    if (layerIndex == 0)
                    {
                        foreach (KeyValuePair<string, InputBindingMeta> tempMeta in bindingDict)
                        {
                            switch (tempMeta.Value.controlType)
                            {
                                case InputBindingMeta.InputControlType.Button:
                                    ButtonNoAction btnNoAction = new ButtonNoAction();
                                    btnNoAction.MappingId = tempMeta.Key;
                                    layer.buttonActionDict.Add(tempMeta.Key, btnNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.DPad:
                                    DPadNoAction dpadNoAction = new DPadNoAction();
                                    dpadNoAction.MappingId = tempMeta.Key;
                                    layer.dpadActionDict.Add(tempMeta.Key, dpadNoAction);
                                    break;
                                case InputBindingMeta.InputControlType.Stick:
                                    {
                                        StickNoAction stickNoAct = new StickNoAction();
                                        stickNoAct.MappingId = tempMeta.Key;
                                        if (knownStickDefinitions.TryGetValue(tempMeta.Key,
                                            out StickDefinition tempDef))
                                        {
                                            stickNoAct.StickDefinition = tempDef;
                                        }
                                        layer.stickActionDict.Add(tempMeta.Key, stickNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Trigger:
                                    {
                                        TriggerNoAction trigNoAct = new TriggerNoAction();
                                        trigNoAct.MappingId = tempMeta.Key;
                                        if (knownTriggerDefinitions.TryGetValue(tempMeta.Key,
                                            out TriggerDefinition tempDef))
                                        {
                                            trigNoAct.TriggerDef = tempDef;
                                        }
                                        layer.triggerActionDict.Add(tempMeta.Key, trigNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Touchpad:
                                    {
                                        TouchpadNoAction touchNoAct = new TouchpadNoAction();
                                        touchNoAct.MappingId = tempMeta.Key;
                                        if (knownTouchpadDefinitions.TryGetValue(tempMeta.Key,
                                            out TouchpadDefinition tempDef))
                                        {
                                            touchNoAct.TouchDefinition = tempDef;
                                        }
                                        layer.touchpadActionDict.Add(tempMeta.Key, touchNoAct);
                                    }

                                    break;
                                case InputBindingMeta.InputControlType.Gyro:
                                    {
                                        GyroNoMapAction gyroNoMapAct = new GyroNoMapAction();
                                        gyroNoMapAct.MappingId = tempMeta.Key;
                                        if (knownGyroSensDefinitions.TryGetValue(tempMeta.Key,
                                            out GyroSensDefinition tempDef))
                                        {
                                            gyroNoMapAct.GyroSensDefinition = tempDef;
                                        }

                                        layer.gyroActionDict.Add(tempMeta.Key, gyroNoMapAct);
                                    }

                                    break;
                                default:
                                    break;
                            }
                        }

                        ButtonNoAction btnNoActionSet = new ButtonNoAction();
                        btnNoActionSet.MappingId = $"{ActionSet.ACTION_SET_ACTION_PREFIX}{set.Index}";
                        layer.actionSetActionDict.Add(btnNoActionSet.MappingId, btnNoActionSet);
                    }

                    layerIndex++;
                }
            }
        }

        private void SyncActionData(Profile tempProfile)
        {
            // Compile convenience List for MapActions instances in layers
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                ActionLayer parentLayer = set.DefaultActionLayer;
                foreach (ActionLayer layer in set.ActionLayers)
                {
                    layer.SyncActions();
                }
            }


            // Prepare initial composite ActionLayer instance using
            // base ActionLayer references
            foreach (ActionSet set in tempProfile.ActionSets)
            {
                //ActionLayer parentLayer = set.DefaultActionLayer;
                set.ClearCompositeLayerActions();
                set.PrepareCompositeLayer();
            }
        }
    }
}