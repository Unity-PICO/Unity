# 自动房间加入功能使用说明

## 功能概述
这个自动化脚本可以让你无需手动输入房间ID、用户ID、APP_ID和TOKEN，只需点击一个按钮就能自动加入RTC视频房间。

## 文件说明

### 新增文件
- `AutoRoomManager.cs` - 自动房间管理器，负责调用API获取房间信息并自动加入

### 修改文件
- `LoginRTC.cs` - 添加了`AutoJoinRoom`方法，并隐藏了输入框
- `Constants.cs` - 会被自动更新APP_ID和TOKEN

## 使用步骤

### 1. 添加AutoRoomManager组件到场景
1. 在Unity编辑器中，找到包含LoginRTC组件的GameObject
2. 添加`AutoRoomManager`组件到同一个GameObject或创建新的GameObject
3. 在AutoRoomManager组件中设置以下引用：
   - `Auto Join Button`: 指向一个UI Button（用于触发自动加入）
   - `Status Text`: 指向一个UI Text（用于显示状态信息，可选）
   - `Login RTC`: 指向LoginRTC组件（如果为空会自动查找）

### 2. 设置UI
1. 创建一个Button，文本设置为"自动加入房间"或类似文字
2. （可选）创建一个Text用于显示状态信息
3. 将这些UI元素拖拽到AutoRoomManager组件的对应字段中

### 3. 运行测试
1. 运行游戏
2. 点击"自动加入房间"按钮
3. 脚本会自动：
   - 调用API获取房间信息
   - 更新Constants中的APP_ID和TOKEN
   - 自动加入房间，无需任何手动输入

## 技术细节

### API调用
- URL: `https://api.coze.cn/v1/audio/rooms`
- 方法: POST
- 认证: Bearer token
- 请求体: `{"bot_id":"7472050308777017384"}`

### 响应处理
脚本会解析API响应中的：
- `app_id` - 自动设置到Constants.APP_ID
- `token` - 自动设置到Constants.TOKEN  
- `room_id` - 用于加入房间
- `uid` - 用作用户ID

### 错误处理
- 网络请求失败
- API返回错误
- JSON解析失败
- 房间加入失败

所有错误都会在Console和状态文本中显示。

## 注意事项

1. **网络连接**: 确保设备有网络连接
2. **API Token**: 确保API token有效且未过期
3. **平台兼容性**: 脚本使用UnityWebRequest，支持所有Unity平台
4. **输入框隐藏**: 原有的房间ID和用户ID输入框会被自动隐藏
5. **调试信息**: 查看Console窗口获取详细的调试信息

## 故障排除

### 常见问题
1. **按钮点击无响应**: 检查AutoRoomManager组件是否正确添加和配置
2. **网络请求失败**: 检查网络连接和API URL
3. **API返回错误**: 检查bot_id和认证token是否正确
4. **加入房间失败**: 检查LoginRTC组件引用是否正确

### 调试步骤
1. 打开Unity Console窗口
2. 点击自动加入按钮
3. 查看Console中的日志信息
4. 根据错误信息进行相应处理