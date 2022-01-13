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

struct ContentView: View {
    @ObservedObject private var setting = Setting()
    @State private var showImagePicker = false
    @State private var showingAlert = false

    var body: some View {
        NavigationView {
            VStack {
                Text("전송할 사진을 선택/촬영해 주세요")
                        .bold()
            }
                    .padding(/*@START_MENU_TOKEN@*/.all/*@END_MENU_TOKEN@*/, /*@START_MENU_TOKEN@*/10/*@END_MENU_TOKEN@*/)
                    .navigationBarTitle(Text("Trash"), displayMode: .inline)
                    .navigationBarItems(leading: NavigationLink(destination: SettingView(setting: setting)) {
                        Text("설정")
                    }, trailing: Button(action: {
                        showImagePicker.toggle()
                    }, label: {
                        Image(systemName: self.setting.selectedIndex == 0 ? "camera" : "photo").imageScale(.large)
                    }).sheet(isPresented: $showImagePicker) {
                        ImagePickerView(sourceType: self.setting.selectedIndex == 0 ? .camera : .photoLibrary, onImagePicked: { image in
                            S3Manager.shared.uploadImage(image: image)
                            self.showingAlert.toggle()
                        })
                    }.alert(isPresented: $showingAlert) {
                        Alert(title: Text("알림"), message: Text("이미지 전송 완료"), dismissButton: .default(Text("확인")))
                    })
        }
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
