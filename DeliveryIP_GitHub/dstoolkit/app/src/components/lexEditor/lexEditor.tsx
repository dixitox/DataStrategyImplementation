import React, { forwardRef, useImperativeHandle, useRef } from "react";
import DefaultTheme from "./themes/default";
import { InitialConfigType, InitialEditorStateType, LexicalComposer } from "@lexical/react/LexicalComposer";
import { RichTextPlugin } from "@lexical/react/LexicalRichTextPlugin";
import { ContentEditable } from "@lexical/react/LexicalContentEditable";
import { HistoryPlugin } from "@lexical/react/LexicalHistoryPlugin";
import { AutoFocusPlugin } from "@lexical/react/LexicalAutoFocusPlugin";
import LexicalErrorBoundary from "@lexical/react/LexicalErrorBoundary";
import ToolbarPlugin from "./plugins/toolbarPlugin";
import { HeadingNode, QuoteNode } from "@lexical/rich-text";
import { TableCellNode, TableNode, TableRowNode } from "@lexical/table";
import { ListItemNode, ListNode } from "@lexical/list";
import { CodeHighlightNode, CodeNode } from "@lexical/code";
import { AutoLinkNode, LinkNode } from "@lexical/link";
import { LinkPlugin } from "@lexical/react/LexicalLinkPlugin";
import { ListPlugin } from "@lexical/react/LexicalListPlugin";
import { MarkdownShortcutPlugin } from "@lexical/react/LexicalMarkdownShortcutPlugin";
import { TRANSFORMERS } from "@lexical/markdown";
import ListMaxIndentLevelPlugin from "./plugins/listMaxIndentLevelPlugin";
import CodeHighlightPlugin from "./plugins/codeHighlightPlugin";
import AutoLinkPlugin from "./plugins/autoLinkPlugin";
import { EditorState } from "lexical";

import { ExportImportPlugin, ExportImportPluginHandle } from "./plugins/exportImportPlugin";
import "./themes/editorStyles.scss";
import { ImageNode } from "./nodes/imageNode";
import ImagePlugin from "./plugins/imagePlugin";

interface LexEditorProps {
    placeholder?: string;
    editorState?: InitialEditorStateType;
    uploadImage?: (base64Image: string | ArrayBuffer | null) => Promise<string>;    // Upload image to server and return the url
}

export interface LexEditorHandle {
    getState: () => EditorState | undefined;
    getHtmlAsync: () => Promise<string>;
    setHtml(html: string): void;
    clear(): void;
}

export const LexEditor = forwardRef<LexEditorHandle, LexEditorProps>((props, ref) => {
    const editorConfig: InitialConfigType = {
        // The editor theme
        theme: DefaultTheme,
        namespace: "dstoolkit",
        // Handling of errors during update
        onError(error: any) {
            throw error;
        },
        // Any custom nodes go here
        nodes: [
            HeadingNode,
            ListNode,
            ListItemNode,
            QuoteNode,
            CodeNode,
            CodeHighlightNode,
            TableNode,
            TableCellNode,
            TableRowNode,
            AutoLinkNode,
            LinkNode,
            ImageNode,
        ],
        editorState: props.editorState,
    };

    const editorStateRef = useRef<EditorState>();
    const ExportImportPluginRef = useRef<ExportImportPluginHandle>(null);

    useImperativeHandle(ref, () => ({
        getState: () => editorStateRef.current,
        getHtmlAsync: () => ExportImportPluginRef.current!.getHtmlAsync(),
        setHtml: (html: string) => ExportImportPluginRef.current!.setHtml(html),
        clear: () => ExportImportPluginRef.current!.clear(),
    }));

    // function onChange(editorState: EditorState, _editor: LexicalEditor) {
    //     editorStateRef.current = editorState;
    // }

    return (
        <LexicalComposer initialConfig={editorConfig}>
            <div className="editor-container">
                <ToolbarPlugin uploadImage={props.uploadImage} />
                <div className="editor-inner">
                    <RichTextPlugin
                        contentEditable={<ContentEditable className="editor-input" />}
                        placeholder={
                            props.placeholder ? <div className="editor-placeholder">{props.placeholder}</div> : null
                        }
                        ErrorBoundary={LexicalErrorBoundary}
                    />
                    <HistoryPlugin />

                    <AutoFocusPlugin />
                    <CodeHighlightPlugin />
                    <ListPlugin />
                    <LinkPlugin />
                    <AutoLinkPlugin />
                    <ListMaxIndentLevelPlugin maxDepth={7} />
                    <MarkdownShortcutPlugin transformers={TRANSFORMERS} />
                    {/* Test output: */}
                    {/* <TreeViewPlugin /> */}
                    {/* <OnChangePlugin onChange={onChange} ignoreSelectionChange /> */}
                    <ExportImportPlugin ref={ExportImportPluginRef} />
                    <ImagePlugin />
                </div>
            </div>
        </LexicalComposer>
    );
});

export default LexEditor;
