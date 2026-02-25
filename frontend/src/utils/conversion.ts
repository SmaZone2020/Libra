// 数据转换工具函数

/**
 * 将Base64字符串转换为UTF-8字符串
 * @param base64 Base64编码的字符串
 * @returns 解码后的UTF-8字符串
 */
export function base64ToUtf8(base64: string): string {
  try {
    const binary = atob(base64);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
    return new TextDecoder().decode(bytes);
  } catch {
    return '';
  }
}
