import { useEffect } from "react";
import { $getListDepth, $isListItemNode, $isListNode, ListNode } from "@lexical/list";
import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import {
    $getSelection,
    $isElementNode,
    $isRangeSelection,
    INDENT_CONTENT_COMMAND,
    COMMAND_PRIORITY_HIGH,
    LexicalNode,
} from "lexical";

function getElementNodesInSelection(selection: any) {
    const nodesInSelection = selection.getNodes();

    if (nodesInSelection.length === 0) {
        return new Set([selection.anchor.getNode().getParentOrThrow(), selection.focus.getNode().getParentOrThrow()]);
    }

    return new Set(nodesInSelection.map((n: any) => ($isElementNode(n) ? n : n.getParentOrThrow())));
}

function isIndentPermitted(maxDepth: number) {
    const selection = $getSelection();

    if (!$isRangeSelection(selection)) {
        return false;
    }

    const elementNodesInSelection = getElementNodesInSelection(selection);

    let totalDepth = 0;

    for (const elementNode of elementNodesInSelection) {
        if ($isListNode(elementNode as LexicalNode | null | undefined)) {
            totalDepth = Math.max($getListDepth(elementNode as ListNode) + 1, totalDepth);
        } else if ($isListItemNode(elementNode as LexicalNode | null | undefined)) {
            const parent = (elementNode as LexicalNode)?.getParent();
            if (!$isListNode(parent)) {
                throw new Error("ListMaxIndentLevelPlugin: A ListItemNode must have a ListNode for a parent.");
            }

            totalDepth = Math.max($getListDepth(parent) + 1, totalDepth);
        }
    }

    return totalDepth <= maxDepth;
}

export default function ListMaxIndentLevelPlugin({ maxDepth }: { maxDepth: number }) {
    const [editor] = useLexicalComposerContext();

    useEffect(() => {
        return editor.registerCommand(
            INDENT_CONTENT_COMMAND,
            () => !isIndentPermitted(maxDepth ?? 7),
            COMMAND_PRIORITY_HIGH
        );
    }, [editor, maxDepth]);

    return null;
}
