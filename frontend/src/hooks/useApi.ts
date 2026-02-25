import { useState, useCallback } from 'react';

/**
 * 自定义hook，用于处理API调用
 * @returns API调用相关的状态和方法
 */
export function useApi() {
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  /**
   * 执行API调用
   * @param apiFn API函数
   * @param args API函数参数
   * @returns API调用结果
   */
  const execute = useCallback(async <T,>(apiFn: (...args: any[]) => Promise<T>, ...args: any[]): Promise<T> => {
    try {
      setLoading(true);
      setError('');
      const result = await apiFn(...args);
      return result;
    } catch (err: any) {
      setError(err.message || 'API调用失败');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * 清除错误
   */
  const clearError = useCallback(() => {
    setError('');
  }, []);

  return {
    loading,
    error,
    execute,
    clearError,
  };
}
