using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.DPadActions;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.SteamControllerLibrary;
using DS4MapperTest.StickActions;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.TriggerActions;
using Newtonsoft.Json;

namespace DS4MapperUnitTests
{
    [TestClass]
    public class MappingTests : BindingHelperBase
    {
        private string xinputProfileJson;
        //private TestMapper mapper;

        public MappingTests()
        {
            #region profileJSON
            xinputProfileJson = @"{
  ""Name"": ""XInput"",
  ""Description"": ""X360"",
  ""Creator"": ""ryochan7"",
  ""CreationDate"": ""2022-04-07T16:23:39+0000"",
  ""ActionSets"": [
    {
      ""Index"": ""0"",
      ""Name"": ""Set 1"",
      ""Description"": ""Only ActionSets"",
      ""ActionLayers"": [
        {
          ""Index"": ""0"",
          ""Name"": ""Default"",
          ""Description"": ""Only Action Layer"",
          ""MappedActions"": [
            {
              ""Id"": ""0"",
              ""Name"": ""LS Action"",
              ""ActionMode"": ""TouchStickTranslateAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_LS"",
                ""DeadZone"": 0.05,
                ""AntiDeadZone"": 0.35,
                ""MaxZone"": 0.7,
                ""Rotation"": 0
              }
            },
            {
              ""Id"": ""1"",
              ""Name"": ""RS Action"",
              ""ActionMode"": ""TouchMouseJoystickAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_RS"",
              }
            },
            {
              ""Id"": ""2"",
              ""Name"": ""DPad Action"",
              ""ActionMode"": ""DPadTranslateAction"",
              ""Settings"":
              {
                ""OutputDPad"": ""X360_DPAD""
              }
            },
            {
              ""Id"": ""3"",
              ""Name"": ""X360 A Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_A""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""4"",
              ""Name"": ""X360 B Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_B""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""5"",
              ""Name"": ""X360 X Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""6"",
              ""Name"": ""X360 Y Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Y""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""7"",
              ""Name"": ""X360 LB Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_LB""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""8"",
              ""Name"": ""X360 RB Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_RB""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""9"",
              ""Name"": ""X360 LT Button"",
              ""ActionMode"": ""TriggerTranslateAction"",
              ""Settings"":
              {
                ""OutputTrigger"": ""X360_LT""
              }
            },
            {
              ""Id"": ""10"",
              ""Name"": ""X360 RT Button"",
              ""ActionMode"": ""TriggerTranslateAction"",
              ""Settings"":
              {
                ""OutputTrigger"": ""X360_RT""
              }
            },
            {
              ""Id"": ""11"",
              ""Name"": ""X360 Back Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Back""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""12"",
              ""Name"": ""X360 Start Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Start""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""13"",
              ""Name"": ""X360 ThumbL Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_ThumbL""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""14"",
              ""Name"": ""X360 ThumbR Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_ThumbR""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""15"",
              ""Name"": ""X"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""16"",
              ""Name"": ""A"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_A""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""17"",
              ""Name"": ""StickDPAD"",
              ""ActionMode"": ""StickPadAction"",
              ""Bindings"":
              {
                ""Up"":
                {
                    ""Name"": ""Dpad Up"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_UP""
                          }
                        ]
                      }
                    ]
                },
                ""Down"":
                {
                    ""Name"": ""Dpad Down"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_DOWN""
                          }
                        ]
                      }
                    ]
                },
                ""Left"":
                {
                    ""Name"": ""Dpad Left"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_LEFT""
                          }
                        ]
                      }
                    ]
                },
                ""Right"":
                {
                    ""Name"": ""Dpad Right"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_RIGHT""
                          }
                        ]
                      }
                    ]
                },
              },
              ""Settings"":
              {
                ""DeadZone"": 0.3,
                ""DiagonalRange"": 45,
              }
            },
            {
              ""Id"": ""18"",
              ""Name"": ""X360 Start Button"",
              ""ActionMode"": ""TriggerButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_Start""
                    }
                  ]
                }
              ]
            },
            {
              ""Id"": ""19"",
              ""Name"": ""TouchPAD"",
              ""ActionMode"": ""TouchActionPadAction"",
              ""Bindings"":
              {
                ""Up"":
                {
                    ""Name"": ""Dpad Up"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_UP""
                          }
                        ]
                      }
                    ]
                },
                ""Down"":
                {
                    ""Name"": ""Dpad Down"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_DOWN""
                          }
                        ]
                      }
                    ]
                },
                ""Left"":
                {
                    ""Name"": ""Dpad Left"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_LEFT""
                          }
                        ]
                      }
                    ]
                },
                ""Right"":
                {
                    ""Name"": ""Dpad Right"",
                    ""Functions"": [
                      {
                        ""Type"": ""NormalPress"",
                        ""OutputActions"": [
                          {
                            ""Type"": ""GamepadControl"",
                            ""PadOutput"": ""X360_DPAD_RIGHT""
                          }
                        ]
                      }
                    ]
                },
              },
              ""Settings"":
              {
                ""DeadZone"": 0.05,
                ""DiagonalRange"": 30,
              }
            },
            {
              ""Id"": ""20"",
              ""Name"": ""LS Action"",
              ""ActionMode"": ""StickTranslateAction"",
              ""Settings"":
              {
                ""OutputStick"": ""X360_LS"",
                ""DeadZone"": 0.10,
                ""AntiDeadZone"": 0.24,
                ""Rotation"": 0
              }
            }
          ]
        },
        {
          ""Index"": ""1"",
          ""Name"": ""Test Layer"",
          ""Description"": ""Action Layer"",
          ""MappedActions"": [
            {
              ""Id"": ""0"",
              ""Name"": ""X360 X Button"",
              ""ActionMode"": ""ButtonAction"",
              ""Functions"": [
                {
                  ""Type"": ""NormalPress"",
                  ""OutputActions"": [
                    {
                      ""Type"": ""GamepadControl"",
                      ""PadOutput"": ""X360_X""
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
      ""ActionSet"": ""0"",
      ""ActionLayer"": ""0"",
      ""InputMappings"": [
        {
          ""Input"": ""LeftTouchpad"",
          ""Action"": ""0""
        },
        {
          ""Input"": ""RightTouchpad"",
          ""Action"": ""1""
        },
        {
          ""Input"": ""A"",
          ""Action"": ""3""
        },
        {
          ""Input"": ""B"",
          ""Action"": ""4""
        },
        {
          ""Input"": ""X"",
          ""Action"": ""5""
        },
        {
          ""Input"": ""Y"",
          ""Action"": ""6""
        },
        {
          ""Input"": ""LeftGrip"",
          ""Action"": ""15""
        },
        {
          ""Input"": ""RightGrip"",
          ""Action"": ""16""
        },
        {
          ""Input"": ""LShoulder"",
          ""Action"": ""7""
        },
        {
          ""Input"": ""RShoulder"",
          ""Action"": ""8""
        },
        {
          ""Input"": ""LT"",
          ""Action"": ""9""
        },
        {
          ""Input"": ""RT"",
          ""Action"": ""10""
        },
        {
          ""Input"": ""Back"",
          ""Action"": ""11""
        },
        {
          ""Input"": ""Start"",
          ""Action"": ""12""
        },
        {
          ""Input"": ""LeftPadClick"",
          ""Action"": ""13""
        },
        {
          ""Input"": ""RightPadClick"",
          ""Action"": ""14""
        },
        {
          ""Input"": ""Stick"",
          ""Action"": ""17""
        }
      ]
    },
    {
      ""ActionSet"": ""0"",
      ""ActionLayer"": ""1"",
      ""InputMappings"": [
        {
          ""Input"": ""Y"",
          ""Action"": ""0""
        }
      ]
    }
  ]
}

";
            #endregion

            mapper = new TestMapper();
        }

        [TestMethod]
        public void XInputOutputTests()
        {
            Profile tempProfile = new Profile();
            // Need to create mapper using profile
            mapper = new TestMapper(tempProfile);
            // Purge default action set data
            tempProfile.ActionSets.Clear();
            List<ProfileActionsMapping> tempMappings = null;

            ProfileSerializer profileSerializer = new ProfileSerializer(tempProfile);

            // Check that profile can be read at all
            string json = xinputProfileJson;
            JsonConvert.PopulateObject(json, profileSerializer);

            // Migrate profile data from serializer to Profile instance
            profileSerializer.PopulateProfile();
            tempProfile.ResetAliases();
            // Need list of currently bound actions
            tempMappings = profileSerializer.ActionMappings;

            // Map actions to dictionary and mapping string ID
            FillMappingProfileInitialData(tempProfile, tempMappings);
            // Sync action layer dictionaries
            SyncActionData(tempProfile);

            // Populate input state struct with mock data
            SteamControllerState inputState = new SteamControllerState()
            {
                A = true,
                // Move stick to DownRight
                LX = 27000,
                LY = -27000,
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState outputState);
                Assert.AreEqual(true, outputState.BtnSouth); // A
                Assert.AreEqual(true, outputState.DpadDown); // LY
                Assert.AreEqual(true, outputState.DpadRight); // LX
            }

            // Populate input state struct with mock data
            inputState = new SteamControllerState()
            {
                X = true,
                LeftPad = new SteamControllerState.TouchPadInfo()
                {
                    X = 32767,
                    Y = 0,
                    Touch = true,
                    Click = false,
                },
            };

            {
                // Run mapper routine and check output state
                mapper.Reader_Report(inputState, out IntermediateState outputState);
                Assert.AreEqual(true, outputState.BtnWest); // X
                Assert.AreEqual(false, outputState.BtnSouth); // A (check release)
                Assert.AreEqual(1.0, outputState.LX); // LeftPad X
                Assert.AreEqual(0.0, outputState.LY); // LeftPad Y
                Assert.AreEqual(false, outputState.BtnThumbL); // LeftPad Click
            }
        }
    }
}
