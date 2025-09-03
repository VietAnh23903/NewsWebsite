/*
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function( config )
{
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
};
CKEDITOR.on('dialogDefinition', function (event) {
    var editor = event.editor;
    var dialogDefinition = event.data.definition;
    console.log(event.editor);
    var dialogName = event.data.name;

    var tabCount = dialogDefinition.contents.length;
    for (var i = 0; i < tabCount; i++) {
        var browseButton = dialogDefinition.contents[i].get('browse');

        if (browseButton !== null) {
            browseButton.hidden = false;
            browseButton.onClick = function (dialog, i) {
                editor._.filebrowserSe = this;
                var fm = $('<div id="elfinder" style="z-index:99999" />').dialogelfinder({
                    url: '/quan-ly-tep-tin/connector',
                    baseUrl: "/elfinder/",
                    width: 840,
                    height: 450,
                    destroyOnClose: true,
                    title: 'Quản lý tệp tin',
                    getFileCallback: function (files, fm) {
                        CKEDITOR.tools.callFunction(editor._.filebrowserFn, files[0].url);
                    },
                    commandsOptions: {
                        getfile: {
                            multiple: true,
                            oncomplete: 'close',
                            folders: false
                        }
                    }
                }).dialogelfinder('instance');
            }
        }
    }
});