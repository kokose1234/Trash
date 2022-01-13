//  Copyright 2021 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

import SwiftUI

struct SettingView: View {
    @ObservedObject var setting: Setting
    
    var types = ["카메라", "사진 앨범"]
    
    var body: some View {
        VStack{
            Text("이미지 소스")
                .padding(.top, 15)
            Picker(selection: $setting.selectedIndex, label: Text("")){
                ForEach(0..<types.count){
                    Text(self.types[$0])
                }
            }
            .padding(.top, -50)
            .padding(.bottom, -50)
            Spacer()
        }
        .navigationBarTitle(Text("설정"))
    }
}

struct SettingView_Previews: PreviewProvider {
    static var previews: some View {
        SettingView(setting: Setting())
    }
}
