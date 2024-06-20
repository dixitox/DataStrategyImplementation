import { forwardRef, useImperativeHandle } from "react";
import { $generateHtmlFromNodes, $generateNodesFromDOM } from "@lexical/html";
import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { $getRoot, $setSelection } from "lexical";
import { $isDecoratorNode } from "lexical";
import { $isElementNode } from "lexical";

export interface ExportImportPluginHandle {
    getHtmlAsync: () => Promise<string>;
    setHtml(html: string): void;
    clear(): void;
}

interface ExportImportPluginProps {}

export const ExportImportPlugin = forwardRef<ExportImportPluginHandle, ExportImportPluginProps>(
    (_props, ref): JSX.Element | null => {
        const [editor] = useLexicalComposerContext();

        useImperativeHandle(ref, () => ({
            getHtmlAsync: getHtmlAsync,
            setHtml: setHtml,
            clear: clear,
        }));

        function getHtmlAsync(): Promise<string> {
            return new Promise((resolve, reject) => {
                try {
                    editor.getEditorState().read(() => {
                        const raw = $generateHtmlFromNodes(editor, null);
                        resolve(raw);
                    });
                } catch (error) {
                    reject(error);
                }
            });
        }

        function setHtml(html: string): void {
            const parser = new DOMParser();
            const dom = parser.parseFromString(html, "text/html");
            editor.update(() => {
                // Once we have the DOM instance we use it to generate LexicalNodes.
                const nodes = $generateNodesFromDOM(editor, dom);
                const root = $getRoot();
                root.clear();
                $setSelection(null);
                nodes.forEach((node: any) => {
                    if ($isElementNode(node) || $isDecoratorNode(node)) {
                        root.append(node);
                    }
                });
            });
        }

        function clear(): void {
            editor.update(() => {
                $setSelection(null);
                const root = $getRoot();
                root.clear();
            });
        }

        return null;
    }
);

export default ExportImportPlugin;
