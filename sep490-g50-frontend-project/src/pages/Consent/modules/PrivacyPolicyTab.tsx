import { message, Typography } from "antd";
import { useEffect, useState } from "react";
import {PolicyViewModel } from "../models/PolicyViewModel";
import AxiosClient from "../../../configs/axiosConfig";
import DOMPurify from 'dompurify'; // Import DOMPurify

const PrivacyPolicyTab: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const [policy, setPolicy] = useState<PolicyViewModel>();
    const [htmlContent, setHtmlContent] = useState('');

    const transformPToText = (html: string): string => {
        try {
            // Sanitize the HTML FIRST
            const sanitizedHtml = DOMPurify.sanitize(html);

            // Create a temporary div to parse the HTML
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = sanitizedHtml;

            // Replace all <p> tags with <text>  (Crucially, use `svg` namespace correctly)
            const paragraphs = tempDiv.getElementsByTagName('p');

            // Convert HTMLCollection to array and iterate in reverse
            // (to avoid live DOM collection issues)
            Array.from(paragraphs).reverse().forEach(p => {
                // Use the correct namespace for SVG elements
                const textElement = document.createElementNS('http://www.w3.org/2000/svg', 'text');

                // Copy all attributes from <p> to <text>
                Array.from(p.attributes).forEach(attr => {
                    textElement.setAttribute(attr.name, attr.value);
                });

                // Add line spacing classes.  Use SVG-compatible classes or inline styles.
                // Ant Design classes are for HTML, not SVG
                textElement.setAttribute('class', 'svg-text-class'); // Or inline styles
                textElement.setAttribute('style', 'display: block; margin-bottom: 16px; text-align: left;');

                // Process <strong> tags within the <p> before moving content
                const strongTags = p.getElementsByTagName('strong');
                Array.from(strongTags).forEach(strong => {
                    // Create a <tspan> element
                    const tspan = document.createElementNS('http://www.w3.org/2000/svg', 'tspan');
                    tspan.setAttribute('style', 'text-anchor: middle; alignment-baseline: central; display:block; text-align:center; font-weight: bold'); // SVG-compatible centering

                    // Move the contents of <strong> to <tspan>
                    while (strong.firstChild) {
                        tspan.appendChild(strong.firstChild);
                    }

                    // Replace <strong> with <tspan>
                    strong.parentNode?.replaceChild(tspan, strong);
                });

                // Move all child nodes to the new <text> element.  Important:  Handle text nodes.
                while (p.firstChild) {
                    const child = p.firstChild;
                    if (child.nodeType === Node.TEXT_NODE) {
                        // Handle text nodes specifically (very important for SVG text)
                        textElement.textContent = child.textContent; // Set the text content of the SVG text element
                        p.removeChild(child); //Remove it from p tag since textcontent has been moved
                    } else {
                        // For other nodes, append as usual (though SVG may not support all HTML nodes)
                        textElement.appendChild(child);
                    }
                }
                // Replace the <p> with <text>
                p.parentNode?.replaceChild(textElement, p);
            });

            return tempDiv.innerHTML; // Return the transformed HTML
        } catch (error) {
            console.error("Error transforming HTML:", error); // Log the error for debugging
            return ''; 
        }
    };

    const fetchPolicy = async () => {
        setLoading(true);
        try {
            const response = await AxiosClient.get(`/PrivacyPolicy/get-policy`);
            if (response.status === 200) {
                setPolicy(response.data);

                // Transform <p> to <text>
                const transformedContent = transformPToText(response.data.content);
                setHtmlContent(transformedContent);
            }
        } catch (error) {
            message.error("API call failed.");
            setHtmlContent("<h3>Policy failed to load</h3>");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPolicy();
    }, []);

    return (
        <section
            className=""
            dangerouslySetInnerHTML={{
                __html: htmlContent
            }}
        />
    );
};

export default PrivacyPolicyTab;